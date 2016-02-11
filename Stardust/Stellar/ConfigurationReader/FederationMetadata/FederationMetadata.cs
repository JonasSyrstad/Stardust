using System.Xml.Serialization;

namespace Stardust.Stellar.ConfigurationReader.FederationMetadata
{

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata", IsNullable = false)]
    public partial class EntityDescriptor
    {

        private Signature signatureField;

        private EntityDescriptorRoleDescriptor[] roleDescriptorField;

        private EntityDescriptorSPSSODescriptor sPSSODescriptorField;

        private EntityDescriptorIDPSSODescriptor iDPSSODescriptorField;

        private EntityDescriptorContactPerson contactPersonField;

        private string idField;

        private string entityIDField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public Signature Signature
        {
            get
            {
                return this.signatureField;
            }
            set
            {
                this.signatureField = value;
            }
        }

        /// <remarks/>
        [XmlElement("RoleDescriptor")]
        public EntityDescriptorRoleDescriptor[] RoleDescriptor
        {
            get
            {
                return this.roleDescriptorField;
            }
            set
            {
                this.roleDescriptorField = value;
            }
        }

        /// <remarks/>
        public EntityDescriptorSPSSODescriptor SPSSODescriptor
        {
            get
            {
                return this.sPSSODescriptorField;
            }
            set
            {
                this.sPSSODescriptorField = value;
            }
        }

        /// <remarks/>
        public EntityDescriptorIDPSSODescriptor IDPSSODescriptor
        {
            get
            {
                return this.iDPSSODescriptorField;
            }
            set
            {
                this.iDPSSODescriptorField = value;
            }
        }

        /// <remarks/>
        public EntityDescriptorContactPerson ContactPerson
        {
            get
            {
                return this.contactPersonField;
            }
            set
            {
                this.contactPersonField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string entityID
        {
            get
            {
                return this.entityIDField;
            }
            set
            {
                this.entityIDField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [XmlRoot(Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class Signature
    {

        private SignatureSignedInfo signedInfoField;

        private string signatureValueField;

        private SignatureKeyInfo keyInfoField;

        /// <remarks/>
        public SignatureSignedInfo SignedInfo
        {
            get
            {
                return this.signedInfoField;
            }
            set
            {
                this.signedInfoField = value;
            }
        }

        /// <remarks/>
        public string SignatureValue
        {
            get
            {
                return this.signatureValueField;
            }
            set
            {
                this.signatureValueField = value;
            }
        }

        /// <remarks/>
        public SignatureKeyInfo KeyInfo
        {
            get
            {
                return this.keyInfoField;
            }
            set
            {
                this.keyInfoField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfo
    {

        private SignatureSignedInfoCanonicalizationMethod canonicalizationMethodField;

        private SignatureSignedInfoSignatureMethod signatureMethodField;

        private SignatureSignedInfoReference referenceField;

        /// <remarks/>
        public SignatureSignedInfoCanonicalizationMethod CanonicalizationMethod
        {
            get
            {
                return this.canonicalizationMethodField;
            }
            set
            {
                this.canonicalizationMethodField = value;
            }
        }

        /// <remarks/>
        public SignatureSignedInfoSignatureMethod SignatureMethod
        {
            get
            {
                return this.signatureMethodField;
            }
            set
            {
                this.signatureMethodField = value;
            }
        }

        /// <remarks/>
        public SignatureSignedInfoReference Reference
        {
            get
            {
                return this.referenceField;
            }
            set
            {
                this.referenceField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoCanonicalizationMethod
    {

        private string algorithmField;

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoSignatureMethod
    {

        private string algorithmField;

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReference
    {

        private SignatureSignedInfoReferenceTransform[] transformsField;

        private SignatureSignedInfoReferenceDigestMethod digestMethodField;

        private string digestValueField;

        private string uRIField;

        /// <remarks/>
        [XmlArrayItem("Transform", IsNullable = false)]
        public SignatureSignedInfoReferenceTransform[] Transforms
        {
            get
            {
                return this.transformsField;
            }
            set
            {
                this.transformsField = value;
            }
        }

        /// <remarks/>
        public SignatureSignedInfoReferenceDigestMethod DigestMethod
        {
            get
            {
                return this.digestMethodField;
            }
            set
            {
                this.digestMethodField = value;
            }
        }

        /// <remarks/>
        public string DigestValue
        {
            get
            {
                return this.digestValueField;
            }
            set
            {
                this.digestValueField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string URI
        {
            get
            {
                return this.uRIField;
            }
            set
            {
                this.uRIField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReferenceTransform
    {

        private string algorithmField;

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReferenceDigestMethod
    {

        private string algorithmField;

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureKeyInfo
    {

        private SignatureKeyInfoX509Data x509DataField;

        /// <remarks/>
        public SignatureKeyInfoX509Data X509Data
        {
            get
            {
                return this.x509DataField;
            }
            set
            {
                this.x509DataField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureKeyInfoX509Data
    {

        private string x509CertificateField;

        /// <remarks/>
        public string X509Certificate
        {
            get
            {
                return this.x509CertificateField;
            }
            set
            {
                this.x509CertificateField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorRoleDescriptor
    {

        private EntityDescriptorRoleDescriptorKeyDescriptor keyDescriptorField;

        private TokenTypesOfferedTokenType[] tokenTypesOfferedField;

        private ClaimType[] claimTypesOfferedField;

        private SecurityTokenServiceEndpoint securityTokenServiceEndpointField;

        private ClaimType[] claimTypesRequestedField;

        private EndpointReference[] targetScopesField;

        private ApplicationServiceEndpoint applicationServiceEndpointField;

        private PassiveRequestorEndpoint passiveRequestorEndpointField;

        private string protocolSupportEnumerationField;

        private string serviceDisplayNameField;

        /// <remarks/>
        public EntityDescriptorRoleDescriptorKeyDescriptor KeyDescriptor
        {
            get
            {
                return this.keyDescriptorField;
            }
            set
            {
                this.keyDescriptorField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        [XmlArrayItem("TokenType", IsNullable = false)]
        public TokenTypesOfferedTokenType[] TokenTypesOffered
        {
            get
            {
                return this.tokenTypesOfferedField;
            }
            set
            {
                this.tokenTypesOfferedField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        [XmlArrayItem("ClaimType", Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706", IsNullable = false)]
        public ClaimType[] ClaimTypesOffered
        {
            get
            {
                return this.claimTypesOfferedField;
            }
            set
            {
                this.claimTypesOfferedField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public SecurityTokenServiceEndpoint SecurityTokenServiceEndpoint
        {
            get
            {
                return this.securityTokenServiceEndpointField;
            }
            set
            {
                this.securityTokenServiceEndpointField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        [XmlArrayItem("ClaimType", Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706", IsNullable = false)]
        public ClaimType[] ClaimTypesRequested
        {
            get
            {
                return this.claimTypesRequestedField;
            }
            set
            {
                this.claimTypesRequestedField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        [XmlArrayItem("EndpointReference", Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
        public EndpointReference[] TargetScopes
        {
            get
            {
                return this.targetScopesField;
            }
            set
            {
                this.targetScopesField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public ApplicationServiceEndpoint ApplicationServiceEndpoint
        {
            get
            {
                return this.applicationServiceEndpointField;
            }
            set
            {
                this.applicationServiceEndpointField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
        public PassiveRequestorEndpoint PassiveRequestorEndpoint
        {
            get
            {
                return this.passiveRequestorEndpointField;
            }
            set
            {
                this.passiveRequestorEndpointField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string protocolSupportEnumeration
        {
            get
            {
                return this.protocolSupportEnumerationField;
            }
            set
            {
                this.protocolSupportEnumerationField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string ServiceDisplayName
        {
            get
            {
                return this.serviceDisplayNameField;
            }
            set
            {
                this.serviceDisplayNameField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorRoleDescriptorKeyDescriptor
    {

        private KeyInfo keyInfoField;

        private string useField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public KeyInfo KeyInfo
        {
            get
            {
                return this.keyInfoField;
            }
            set
            {
                this.keyInfoField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string use
        {
            get
            {
                return this.useField;
            }
            set
            {
                this.useField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [XmlRoot(Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class KeyInfo
    {

        private KeyInfoX509Data x509DataField;

        /// <remarks/>
        public KeyInfoX509Data X509Data
        {
            get
            {
                return this.x509DataField;
            }
            set
            {
                this.x509DataField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class KeyInfoX509Data
    {

        private string x509CertificateField;

        /// <remarks/>
        public string X509Certificate
        {
            get
            {
                return this.x509CertificateField;
            }
            set
            {
                this.x509CertificateField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    public partial class TokenTypesOfferedTokenType
    {

        private string uriField;

        /// <remarks/>
        [XmlAttribute()]
        public string Uri
        {
            get
            {
                return this.uriField;
            }
            set
            {
                this.uriField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706", IsNullable = false)]
    public partial class ClaimType
    {

        private string displayNameField;

        private string descriptionField;

        private string uriField;

        private bool optionalField;

        /// <remarks/>
        public string DisplayName
        {
            get
            {
                return this.displayNameField;
            }
            set
            {
                this.displayNameField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Uri
        {
            get
            {
                return this.uriField;
            }
            set
            {
                this.uriField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public bool Optional
        {
            get
            {
                return this.optionalField;
            }
            set
            {
                this.optionalField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class SecurityTokenServiceEndpoint
    {

        private EndpointReference endpointReferenceField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference
        {
            get
            {
                return this.endpointReferenceField;
            }
            set
            {
                this.endpointReferenceField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
    [XmlRoot(Namespace = "http://www.w3.org/2005/08/addressing", IsNullable = false)]
    public partial class EndpointReference
    {

        private string addressField;

        private EndpointReferenceMetadata metadataField;

        /// <remarks/>
        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }

        /// <remarks/>
        public EndpointReferenceMetadata Metadata
        {
            get
            {
                return this.metadataField;
            }
            set
            {
                this.metadataField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/08/addressing")]
    public partial class EndpointReferenceMetadata
    {

        private Metadata metadataField;

        /// <remarks/>
        [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
        public Metadata Metadata
        {
            get
            {
                return this.metadataField;
            }
            set
            {
                this.metadataField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
    [XmlRoot(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex", IsNullable = false)]
    public partial class Metadata
    {

        private MetadataMetadataSection metadataSectionField;

        /// <remarks/>
        public MetadataMetadataSection MetadataSection
        {
            get
            {
                return this.metadataSectionField;
            }
            set
            {
                this.metadataSectionField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
    public partial class MetadataMetadataSection
    {

        private MetadataMetadataSectionMetadataReference metadataReferenceField;

        private string dialectField;

        /// <remarks/>
        public MetadataMetadataSectionMetadataReference MetadataReference
        {
            get
            {
                return this.metadataReferenceField;
            }
            set
            {
                this.metadataReferenceField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Dialect
        {
            get
            {
                return this.dialectField;
            }
            set
            {
                this.dialectField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
    public partial class MetadataMetadataSectionMetadataReference
    {

        private string addressField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class ApplicationServiceEndpoint
    {

        private EndpointReference endpointReferenceField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference
        {
            get
            {
                return this.endpointReferenceField;
            }
            set
            {
                this.endpointReferenceField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class PassiveRequestorEndpoint
    {

        private EndpointReference endpointReferenceField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference
        {
            get
            {
                return this.endpointReferenceField;
            }
            set
            {
                this.endpointReferenceField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptor
    {

        private EntityDescriptorSPSSODescriptorKeyDescriptor[] keyDescriptorField;

        private EntityDescriptorSPSSODescriptorSingleLogoutService[] singleLogoutServiceField;

        private string[] nameIDFormatField;

        private EntityDescriptorSPSSODescriptorAssertionConsumerService[] assertionConsumerServiceField;

        private bool wantAssertionsSignedField;

        private string protocolSupportEnumerationField;

        /// <remarks/>
        [XmlElement("KeyDescriptor")]
        public EntityDescriptorSPSSODescriptorKeyDescriptor[] KeyDescriptor
        {
            get
            {
                return this.keyDescriptorField;
            }
            set
            {
                this.keyDescriptorField = value;
            }
        }

        /// <remarks/>
        [XmlElement("SingleLogoutService")]
        public EntityDescriptorSPSSODescriptorSingleLogoutService[] SingleLogoutService
        {
            get
            {
                return this.singleLogoutServiceField;
            }
            set
            {
                this.singleLogoutServiceField = value;
            }
        }

        /// <remarks/>
        [XmlElement("NameIDFormat")]
        public string[] NameIDFormat
        {
            get
            {
                return this.nameIDFormatField;
            }
            set
            {
                this.nameIDFormatField = value;
            }
        }

        /// <remarks/>
        [XmlElement("AssertionConsumerService")]
        public EntityDescriptorSPSSODescriptorAssertionConsumerService[] AssertionConsumerService
        {
            get
            {
                return this.assertionConsumerServiceField;
            }
            set
            {
                this.assertionConsumerServiceField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public bool WantAssertionsSigned
        {
            get
            {
                return this.wantAssertionsSignedField;
            }
            set
            {
                this.wantAssertionsSignedField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string protocolSupportEnumeration
        {
            get
            {
                return this.protocolSupportEnumerationField;
            }
            set
            {
                this.protocolSupportEnumerationField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptorKeyDescriptor
    {

        private KeyInfo keyInfoField;

        private string useField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public KeyInfo KeyInfo
        {
            get
            {
                return this.keyInfoField;
            }
            set
            {
                this.keyInfoField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string use
        {
            get
            {
                return this.useField;
            }
            set
            {
                this.useField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptorSingleLogoutService
    {

        private string bindingField;

        private string locationField;

        /// <remarks/>
        [XmlAttribute()]
        public string Binding
        {
            get
            {
                return this.bindingField;
            }
            set
            {
                this.bindingField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptorAssertionConsumerService
    {

        private string bindingField;

        private string locationField;

        private byte indexField;

        private bool isDefaultField;

        private bool isDefaultFieldSpecified;

        /// <remarks/>
        [XmlAttribute()]
        public string Binding
        {
            get
            {
                return this.bindingField;
            }
            set
            {
                this.bindingField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public byte index
        {
            get
            {
                return this.indexField;
            }
            set
            {
                this.indexField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public bool isDefault
        {
            get
            {
                return this.isDefaultField;
            }
            set
            {
                this.isDefaultField = value;
            }
        }

        /// <remarks/>
        [XmlIgnore()]
        public bool isDefaultSpecified
        {
            get
            {
                return this.isDefaultFieldSpecified;
            }
            set
            {
                this.isDefaultFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorIDPSSODescriptor
    {

        private EntityDescriptorIDPSSODescriptorKeyDescriptor[] keyDescriptorField;

        private EntityDescriptorIDPSSODescriptorSingleLogoutService[] singleLogoutServiceField;

        private string[] nameIDFormatField;

        private EntityDescriptorIDPSSODescriptorSingleSignOnService[] singleSignOnServiceField;

        private Attribute[] attributeField;

        private string protocolSupportEnumerationField;

        /// <remarks/>
        [XmlElement("KeyDescriptor")]
        public EntityDescriptorIDPSSODescriptorKeyDescriptor[] KeyDescriptor
        {
            get
            {
                return this.keyDescriptorField;
            }
            set
            {
                this.keyDescriptorField = value;
            }
        }

        /// <remarks/>
        [XmlElement("SingleLogoutService")]
        public EntityDescriptorIDPSSODescriptorSingleLogoutService[] SingleLogoutService
        {
            get
            {
                return this.singleLogoutServiceField;
            }
            set
            {
                this.singleLogoutServiceField = value;
            }
        }

        /// <remarks/>
        [XmlElement("NameIDFormat")]
        public string[] NameIDFormat
        {
            get
            {
                return this.nameIDFormatField;
            }
            set
            {
                this.nameIDFormatField = value;
            }
        }

        /// <remarks/>
        [XmlElement("SingleSignOnService")]
        public EntityDescriptorIDPSSODescriptorSingleSignOnService[] SingleSignOnService
        {
            get
            {
                return this.singleSignOnServiceField;
            }
            set
            {
                this.singleSignOnServiceField = value;
            }
        }

        /// <remarks/>
        [XmlElement("Attribute", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public Attribute[] Attribute
        {
            get
            {
                return this.attributeField;
            }
            set
            {
                this.attributeField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string protocolSupportEnumeration
        {
            get
            {
                return this.protocolSupportEnumerationField;
            }
            set
            {
                this.protocolSupportEnumerationField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorIDPSSODescriptorKeyDescriptor
    {

        private KeyInfo keyInfoField;

        private string useField;

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public KeyInfo KeyInfo
        {
            get
            {
                return this.keyInfoField;
            }
            set
            {
                this.keyInfoField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string use
        {
            get
            {
                return this.useField;
            }
            set
            {
                this.useField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorIDPSSODescriptorSingleLogoutService
    {

        private string bindingField;

        private string locationField;

        /// <remarks/>
        [XmlAttribute()]
        public string Binding
        {
            get
            {
                return this.bindingField;
            }
            set
            {
                this.bindingField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorIDPSSODescriptorSingleSignOnService
    {

        private string bindingField;

        private string locationField;

        /// <remarks/>
        [XmlAttribute()]
        public string Binding
        {
            get
            {
                return this.bindingField;
            }
            set
            {
                this.bindingField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion", IsNullable = false)]
    public partial class Attribute
    {

        private string nameField;

        private string nameFormatField;

        private string friendlyNameField;

        /// <remarks/>
        [XmlAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string NameFormat
        {
            get
            {
                return this.nameFormatField;
            }
            set
            {
                this.nameFormatField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string FriendlyName
        {
            get
            {
                return this.friendlyNameField;
            }
            set
            {
                this.friendlyNameField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorContactPerson
    {

        private object emailAddressField;

        private object telephoneNumberField;

        private string contactTypeField;

        /// <remarks/>
        public object EmailAddress
        {
            get
            {
                return this.emailAddressField;
            }
            set
            {
                this.emailAddressField = value;
            }
        }

        /// <remarks/>
        public object TelephoneNumber
        {
            get
            {
                return this.telephoneNumberField;
            }
            set
            {
                this.telephoneNumberField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string contactType
        {
            get
            {
                return this.contactTypeField;
            }
            set
            {
                this.contactTypeField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class ClaimTypesRequested
    {

        private ClaimType[] claimTypeField;

        /// <remarks/>
        [XmlElement("ClaimType", Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706")]
        public ClaimType[] ClaimType
        {
            get
            {
                return this.claimTypeField;
            }
            set
            {
                this.claimTypeField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class TargetScopes
    {

        private EndpointReference[] endpointReferenceField;

        /// <remarks/>
        [XmlElement("EndpointReference", Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference[] EndpointReference
        {
            get
            {
                return this.endpointReferenceField;
            }
            set
            {
                this.endpointReferenceField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class TokenTypesOffered
    {

        private TokenTypesOfferedTokenType[] tokenTypeField;

        /// <remarks/>
        [XmlElement("TokenType")]
        public TokenTypesOfferedTokenType[] TokenType
        {
            get
            {
                return this.tokenTypeField;
            }
            set
            {
                this.tokenTypeField = value;
            }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "http://docs.oasis-open.org/wsfed/federation/200706")]
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wsfed/federation/200706", IsNullable = false)]
    public partial class ClaimTypesOffered
    {

        private ClaimType[] claimTypeField;

        /// <remarks/>
        [XmlElement("ClaimType", Namespace = "http://docs.oasis-open.org/wsfed/authorization/200706")]
        public ClaimType[] ClaimType
        {
            get
            {
                return this.claimTypeField;
            }
            set
            {
                this.claimTypeField = value;
            }
        }
    }


}
