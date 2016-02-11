using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public static class ReflectedPropertiesSetter
    {
        internal static void Update(IEnumerable<Claim> claims, TypedClaims typedClaimsObject)
        {
            if (claims == null || !claims.Any())
                return;
            var properties = GetProperties(typedClaimsObject);
            foreach (var p in properties)
            {
                var prop = p.Value;
                var attributes = prop.GetCustomAttributes(true).Where(a => a is TypedClaimAttribute);
                if (!attributes.Any())
                    continue;
                SetProperty(p, (TypedClaimAttribute)attributes.First(), claims, typedClaimsObject);
            }
        }

        private static IEnumerable<KeyValuePair<string, PropertyInfo>> GetProperties(TypedClaims typedClaimsObject)
        {
            var settingsType = typedClaimsObject.GetType();
            var result = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in settingsType.GetProperties())
                result[prop.Name] = prop;
            return result;
        }

        private static void SetProperty(KeyValuePair<string, PropertyInfo> property, TypedClaimAttribute attr, IEnumerable<Claim> claims, TypedClaims typedClaimsObject)
        {
            var selectedClaims = claims.Where(c => c.Type == attr.ClaimType);
            if (!selectedClaims.Any())
                return;
            var type = property.Value.GetValue(typedClaimsObject, null).GetType().GetGenericArguments()[0];
            property.Value.SetValue(typedClaimsObject, SetValue(type, selectedClaims, attr.IsCollection), null);
        }

        private static object SetValue(Type type, IEnumerable<Claim> selectedClaims, bool isCollection)
        {
            if (isCollection)
                return GenerateList(type, selectedClaims);
            return GenerateProperty(type, selectedClaims);
        }

        private static object GenerateProperty(Type genericParamType, IEnumerable<Claim> selectedClaims)
        {
            var claim = selectedClaims.First();
            var constructed = typeof(TypedClaim<>).MakeGenericType(new[] { genericParamType });
            var val = Convert.ChangeType(claim.Value, genericParamType, CultureInfo.CurrentCulture);
            return Activator.CreateInstance(constructed, claim.Type, val);
        }

        private static object GenerateList(Type type, IEnumerable<Claim> selectedClaims)
        {
            var constructedList = typeof(List<>).MakeGenericType(new[] { type });
            var list = (IList)Activator.CreateInstance(constructedList);
            Type listGenericType = type.GetGenericArguments()[0];
            foreach (var c in selectedClaims)
                list.Add(Activator.CreateInstance(typeof(TypedClaim<>).MakeGenericType(new[] { listGenericType }), c.Type, Convert.ChangeType(c.Value, listGenericType, CultureInfo.CurrentCulture)));
            return list;
        }
    }
}