using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    [Serializable]
    public class TypeMappingEntityService : AbstractInitializable
    {
        #region Fields

        private Dictionary<string, string> _clientToServerMappings;

        private string _defaultServerMethodName;

        private bool _ignoreAssemblyVersion = true;

        private Dictionary<string, string> _serverMethodNames;

        private Dictionary<string, string> _serverToClientMappings;

        #endregion Fields

        #region Constructors and Finalizers

        public TypeMappingEntityService()
        {
            this.TypeMappings = new List<TypeMapping>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        [XmlAttribute]
        public string DefaultServerMethodName
        {
            get
            {
                return this._defaultServerMethodName;
            }

            set
            {
                if (this.IsInitialized && this._defaultServerMethodName != value)
                {
                    throw new ObjectAlreadyInitializedException("DefaultServerMethodName cannot be changed after the object is initialized.");
                }

                this._defaultServerMethodName = value;
            }
        }

        [XmlAttribute]
        public bool IgnoreAssemblyVersion
        {
            get
            {
                return this._ignoreAssemblyVersion;
            }

            set
            {
                if (this.IsInitialized && this._ignoreAssemblyVersion != value)
                {
                    throw new ObjectAlreadyInitializedException("IgnoreAssemblyVersion cannot be changed after the object is initialized.");
                }

                this._ignoreAssemblyVersion = value;
            }
        }

        public List<TypeMapping> TypeMappings { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void ApplyClientToServerMappings(MethodInformation info)
        {
            this.applyMappings(info, this._clientToServerMappings);
        }

        public void ApplyServerToClientMappings(MethodInformation info)
        {
            this.applyMappings(info, this._serverToClientMappings);
        }

        public string GetClientTypeByServerType(string serverType)
        {
            return this.getFromDictionary(this._serverToClientMappings, serverType);
        }

        public string GetServerMethodName(string serverType)
        {
            return this.getFromDictionary(this._serverMethodNames, serverType);
        }

        public string GetServerTypeByClientType(string clientType)
        {
            return this.getFromDictionary(this._clientToServerMappings, clientType);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void InitializeProtected()
        {
            if (this.IgnoreAssemblyVersion)
            {
                this._serverToClientMappings = new Dictionary<string, string>(IgnoreAssemblyVersionEqualityComparer.Instance);
                this._clientToServerMappings = new Dictionary<string, string>(IgnoreAssemblyVersionEqualityComparer.Instance);
                this._serverMethodNames = new Dictionary<string, string>(IgnoreAssemblyVersionEqualityComparer.Instance);
            }
            else
            {
                this._serverToClientMappings = new Dictionary<string, string>();
                this._clientToServerMappings = new Dictionary<string, string>();
                this._serverMethodNames = new Dictionary<string, string>();
            }

            foreach (TypeMapping mapping in this.TypeMappings)
            {
                this._serverToClientMappings.Add(mapping.ServerType, mapping.ClientType);
                this._clientToServerMappings.Add(mapping.ClientType, mapping.ServerType);
                if (!string.IsNullOrWhiteSpace(mapping.ServerMethodName))
                {
                    this._serverMethodNames.Add(mapping.ServerType, mapping.ServerMethodName);
                }
            }
        }

        private void applyMappings(MethodInformation info, IDictionary<string, string> dictionary)
        {
            this.Initialize();
            info.ApplyTypeMappings(dictionary);
        }

        private string getFromDictionary(IDictionary<string, string> dictionary, string inputType)
        {
            this.Initialize();
            string resultType = null;
            dictionary.TryGetValue(inputType, out resultType);
            return resultType;
        }

        #endregion Methods
    }
}
