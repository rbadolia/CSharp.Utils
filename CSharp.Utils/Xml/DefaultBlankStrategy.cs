using System.Xml;

namespace CSharp.Utils.Xml
{
    public sealed class DefaultBlankStrategy : IBlankStrategy
    {
        #region Static Fields

        private static readonly DefaultBlankStrategy InstanceObject = new DefaultBlankStrategy();

        #endregion Static Fields

        #region Constructors and Finalizers

        private DefaultBlankStrategy()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static DefaultBlankStrategy Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public bool IsBlank(XmlAttribute attribute)
        {
            return attribute.Value != null && attribute.Value.Length > 1 && attribute.Value[0] == '@' && attribute.Value[attribute.Value.Length - 1] == '@';
        }

        #endregion Public Methods and Operators
    }
}
