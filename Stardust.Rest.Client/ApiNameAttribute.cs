using System;

namespace Stardust.Rest.Client
{
    public class ApiNameAttribute : Attribute
    {
        public string ApplicationId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
        /// </summary>
        public ApiNameAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
        /// </summary>
        public ApiNameAttribute(string apiName)
        {
            ApiName = apiName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
        /// </summary>
        public ApiNameAttribute(string apiName, string applicationId)
        {
            ApplicationId = applicationId;
            ApiName = apiName;
        }

        public string ApiName { get; set; }
    }
}