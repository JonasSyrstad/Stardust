using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class Endpoint
    {
        public override string ToString()
        {
            return Id;
        }

        private List<string> addedValues; 

        public Stardust.Interstellar.ConfigurationReader.Endpoint GetRawConfigData(string environment)
        {
            if (String.Equals(Name, "custom", StringComparison.OrdinalIgnoreCase)) return GetEndpointSettingsForCustomType(environment);
            addedValues=new List<string>();
            var data= new Interstellar.ConfigurationReader.Endpoint
            {
                Address = GetParameterValue("Address", environment),
                Audience = GetAudienceParameterValue("Audience", environment),
                BindingType = Name,
                CertificateValidationMode = GetParameterValue("CertificateValidationMode", environment),
                CloseTimeout = (int)GetParameterValueInt("CloseTimeout", environment),
                Durable = GetParameterValue("Durable", environment),
                EndpointName = Name,
                ExactlyOnce = GetParameterValue("ExactlyOnce", environment).Equals("true", StringComparison.CurrentCultureIgnoreCase),
                Ignore = GetParameterValue("Ignore", environment).Equals("true", StringComparison.CurrentCultureIgnoreCase),
                IssuerActAsAddress = GetParameterValue("IssuerActAsAddress", environment),
                HostNameComparisonMode = GetParameterValue("HostNameComparisonMode", environment),
                IssuerAddress = GetParameterValue("IssuerAddress", environment),
                IssuerMetadataAddress = GetParameterValue("IssuerMetadataAddress", environment),
                IssuerName = GetParameterValue("IssuerName", environment),
                MaxBufferPoolSize = GetParameterValueInt("MaxBufferPoolSize", environment),
                MaxBufferSize = (int)GetParameterValueInt("MaxBufferSize", environment),
                MaxConnections = GetParameterValue("MaxConnections", environment),
                MaxMessageSize = GetParameterValueInt("MaxMessageSize", environment),
                MaxReceivedSize = GetParameterValueInt("MaxReceivedSize", environment),
                MessageFormat = GetParameterValue("MessageFormat", environment),
                TransferMode = GetParameterValueTMode("TransferMode", environment),
                OpenTimeout = (int)GetParameterValueInt("OpenTimeout", environment),
                OverrideSslSecurity = GetParameterValue("OverrideSslSecurity", environment).Equals("true", StringComparison.CurrentCultureIgnoreCase),
                ReaderQuotas = GetParameterValueRQ(),
                ReceiveTimeout = (int)GetParameterValueInt("ReceiveTimeout", environment),
                SendTimeout = (int)GetParameterValueInt("SendTimeout", environment),
                StsAddress = GetParameterValue("IssuerActAsAddress", environment),
                TextEncoding = GetParameterValue("TextEncoding", environment),
                Thumbprint = GetParameterValue("Thumbprint", environment),
                PropertyBag = new Dictionary<string, string>()
            };
            data.PropertyBag = (from p in Parameters where !addedValues.Contains(p.Name) select p).ToDictionary(p => p.Name, p => GetParameterValue(p.Name, environment));
            addedValues = null;
            return data;
        }

        private Interstellar.ConfigurationReader.Endpoint GetEndpointSettingsForCustomType(string environment)
        {
            var endpoint = new Interstellar.ConfigurationReader.Endpoint { EndpointName = Name, BindingType = Name, PropertyBag = new Dictionary<string, string>() };
            foreach (var endpointParameter in Parameters)
            {
                endpoint.PropertyBag.Add(endpointParameter.Name, GetParameterValue(endpointParameter.Name, environment));
            }
            return endpoint;
        }


        public void SetFromRawData(Interstellar.ConfigurationReader.Endpoint endpoint, ConfigurationContext repository)
        {
            CreateParameter("Address", endpoint.Address, repository);
            CreateParameter("Audience", endpoint.Audience, repository);
            CreateParameter("CertificateValidationMode", endpoint.CertificateValidationMode, repository);
            CreateParameter("CloseTimeout", endpoint.CloseTimeout, repository);
            CreateParameter("Durable", endpoint.Durable, repository);
            CreateParameter("ExactlyOnce", endpoint.ExactlyOnce, repository);
            CreateParameter("Ignore", endpoint.Ignore, repository);
            CreateParameter("IssuerActAsAddress", endpoint.IssuerActAsAddress, repository);
            CreateParameter("HostNameComparisonMode", endpoint.HostNameComparisonMode, repository);
            CreateParameter("IssuerAddress", endpoint.IssuerAddress, repository);
            CreateParameter("IssuerMetadataAddress", endpoint.IssuerMetadataAddress, repository);
            CreateParameter("IssuerName", endpoint.IssuerName, repository);
            CreateParameter("MaxBufferPoolSize", endpoint.MaxBufferPoolSize, repository);
            CreateParameter("MaxBufferSize", endpoint.MaxBufferSize, repository);
            CreateParameter("MaxConnections", endpoint.MaxConnections, repository);
            CreateParameter("MaxMessageSize", endpoint.MaxMessageSize, repository);
            CreateParameter("MaxReceivedSize", endpoint.MaxReceivedSize, repository);
            CreateParameter("MessageFormat", endpoint.MessageFormat, repository);
            CreateParameter("TransferMode", endpoint.TransferMode, repository);
            CreateParameter("OpenTimeout", endpoint.OpenTimeout, repository);
            CreateParameter("OverrideSslSecurity", endpoint.OverrideSslSecurity, repository);
            CreateParameter("ReceiveTimeout", endpoint.ReceiveTimeout, repository);
            CreateParameter("SendTimeout", endpoint.SendTimeout, repository);
            CreateParameter("StsAddress", endpoint.StsAddress, repository);
            CreateParameter("TextEncoding", endpoint.TextEncoding, repository);
            CreateParameter("Thumbprint", endpoint.Thumbprint, repository);
        }

        private void CreateParameter(string name, bool value, ConfigurationContext repository)
        {
            CreateParameter(name, value.ToString().ToLower(), repository);
        }

        private void CreateParameter(string name, TransferMode value, ConfigurationContext repository)
        {
            CreateParameter(name, value.ToString(), repository);
        }

        private void CreateParameter(string name, long value, ConfigurationContext repository)
        {
            CreateParameter(name, value.ToString(CultureInfo.InvariantCulture), repository);
        }

        private void CreateParameter(string name, string value, ConfigurationContext repository)
        {
            var param = GetParameter(name, repository);
            if (param.IsNull()) throw new Exception(string.Format("Cannot find parameter: {0}", name));
            param.ItemValue = value;
        }

        private IEndpointParameter GetParameter(string name, ConfigurationContext repository)
        {
            try
            {
                var item = Parameters.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (item.IsNull())
                {
                    item = repository.EndpointParameters.Create();
                    item.Name = name;
                    item.Endpoint = this;
                    item.EndpointNameId = Id;
                    item.ConfigurableForEachEnvironment = ServiceDescriptionExtentions.ParameterIsEnvironmental(name, Name);
                    item.IsPerService = ServiceDescriptionExtentions.ParameterIsEnvironmentalPrService(name);
                }
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to create {3}/{2}/{0}: {1}", name, ex.Message, Name, ServiceDescription.Name), ex);
            }
        }

        private XmlDictionaryReaderQuotas GetParameterValueRQ()
        {
            return XmlDictionaryReaderQuotas.Max;
        }

        private TransferMode GetParameterValueTMode(string name, string environment)
        {
            var value = GetParameterValue(name, environment);
            TransferMode result;
            return Enum.TryParse(value, out result) ? result : TransferMode.Buffered;
        }

        private long GetParameterValueInt(string name, string environment)
        {
            var value = GetParameterValue(name, environment);
            long result;
            return long.TryParse(value, out result) ? result : 0;
        }

        private string GetParameterValue(string name, string environment)
        {
            if(addedValues!=null) addedValues.Add(name);
            var param = Parameters.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (param.IsNull()) return "";
            if (!param.ConfigurableForEachEnvironment) return param.Value;
            string value;
            if (ServiceDescription.ServiceHost.IsInstance())
            {
                var shParam = ServiceDescription.ServiceHost.Parameters.SingleOrDefault(sp => string.Equals(sp.Name, param.Name, StringComparison.OrdinalIgnoreCase));
                if (shParam.IsInstance() && shParam.IsEnvironmental)
                {
                    try
                    {
                        var fromServiceHost = ServiceDescription.ConfigSet.Environments.Single(e => e.Name.Equals(environment,StringComparison.OrdinalIgnoreCase))
                                        .SubstitutionParameters.SingleOrDefault(
                                            sp => string.Equals(sp.Name, string.Format("{0}_{1}", ServiceDescription.Name + param.Name), StringComparison.OrdinalIgnoreCase));
                        if(fromServiceHost.IsSecure)
                            return string.Format(param.Value, fromServiceHost.Value.Decrypt(KeyHelper.SharedSecret));
                        return string.Format(param.Value, fromServiceHost.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return !param.EnvironmentalValue.TryGetValue(environment, out value) ? param.Value : value;
        }

        private string GetAudienceParameterValue(string audience, string environment)
        {
            var param = Parameters.SingleOrDefault(x => string.Equals(x.Name, audience, StringComparison.OrdinalIgnoreCase));
            if (param.IsNull()) return "";
            var audienceFromSH = GetAddressPartFromServiceHost(environment);
            if (audienceFromSH.ContainsCharacters()) return string.Format(param.Value, audienceFromSH);
            var substitutionvalue =
                this.ServiceDescription.ConfigSet.Environments.Single(x => x.Name == environment)
                    .SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, ServiceDescription.Name + "_Address", StringComparison.OrdinalIgnoreCase));
            if (substitutionvalue.IsNull()) return param.Value;
            return string.Format(param.Value, substitutionvalue.Value);

        }

        private string GetAddressPartFromServiceHost(string environment)
        {
            if (ServiceDescription.ServiceHost.IsNull()) return null;
            var shParam = ServiceDescription.ServiceHost.Parameters.SingleOrDefault(p => string.Equals(p.Name, "Address", StringComparison.OrdinalIgnoreCase));
            if (shParam.IsNull() || shParam.Value.IsNullOrWhiteSpace()) return null;
            var env = ServiceDescription.ConfigSet.Environments.Single(e => string.Equals(e.Name, environment, StringComparison.OrdinalIgnoreCase));
            var replacedShValue =env.SubstitutionParameters.SingleOrDefault(p => string.Equals(p.Name, string.Format("{0}_Address", ServiceDescription.ServiceHost.Name), StringComparison.OrdinalIgnoreCase));
            return string.Format(GetAudienceFromEnvironment(env), replacedShValue.Value);
        }

        private static string GetAudienceFromEnvironment(IEnvironment env)
        {
            
            var val= env.SubstitutionParameters.SingleOrDefault(sp => string.Equals(sp.Name, "Audience", StringComparison.OrdinalIgnoreCase));
            if (val != null) return "{0}";
            else return val.Value;
        }
    }
}