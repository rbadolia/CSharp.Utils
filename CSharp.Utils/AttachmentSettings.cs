using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using CSharp.Utils.Configuration;
using CSharp.Utils.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils
{
    public sealed class AttachmentSettings : IConfigurable
    {
        #region Static Fields

        private static readonly AttachmentSettings InstanceObject = new AttachmentSettings();

        private HashSet<string> _hashSet = null;

        #endregion Static Fields

        #region Constructors and Finalizers

        private AttachmentSettings()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static AttachmentSettings Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public long? MaxAttachmentSize { get; set; }

        #endregion Public Properties

        public bool ShouldAllowMimeType(string mimeType)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(mimeType, "mimeType");
            if (this._hashSet.Count == 0)
            {
                return true;
            }

            return this._hashSet.Contains(mimeType);
        }

        public void ValidateAttachment(HttpPostedFile postedFile)
        {
            if (!ShouldAllowMimeType(postedFile.ContentType))
            {
                throw new InvalidDataException(string.Format(
                    "[{0}] file mime type is not supported by the application.", postedFile.ContentType));
            }

            if (postedFile.ContentLength == 0)
            {
                throw new InvalidDataException("Empty file upload is not supported.");
            }

            if (this.MaxAttachmentSize != null)
            {
                if (postedFile.ContentLength > this.MaxAttachmentSize.Value)
                {
                    throw new InvalidDataException(
                        string.Format("Can not upload a file having size greater than [{0}] bytes)", 
                            MaxAttachmentSize.Value));
                }
            }
        }

        public void Configure(XmlNode configurationNode, IObjectInstantiator instantiator = null)
        {
            ComponentBuilder.SetComponentPropertiesFromAttributes(this, configurationNode, instantiator);
            var allowedMimeTypes = new List<string>();

            ComponentBuilder.PopulateList(allowedMimeTypes, configurationNode.ChildNodes[0], instantiator);
            this._hashSet = new HashSet<string>(allowedMimeTypes, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
