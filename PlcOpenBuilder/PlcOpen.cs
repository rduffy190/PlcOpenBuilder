using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace PlcOpenBuilder
{
    public class PlcOpen
    {
        public PlcOpen(string fileName)
        {
            _doc = new System.Xml.XmlDocument();
            _doc.Load(fileName);
            _nsManager = new System.Xml.XmlNamespaceManager(_doc.NameTable); 
            TextReader stream = new StringReader(Properties.Resources.tc6_xml_v201);
            _schema = XmlSchema.Read(stream, validateScheme);
            //foreach (var names in _schema.Namespaces.ToArray())
            // {
            //     _nsManager.AddNamespace(names.Name, names.Namespace);
            // }
            _elementaryTypes = _schema.Items.OfType<XmlSchemaGroup>().Where(x => x.Name == "elementaryTypes").First();
            _nsManager.AddNamespace("ns1", "http://www.plcopen.org/xml/tc6_0200");
            _nsManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        }

        private void validateScheme(object? sender, ValidationEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void AddInput(string blockName,string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:inputVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));

        }
        public void AddOutput(string blockName, string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:outputVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));
        }
        public void AddVar(string blockName, string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:localVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));
        }
        public void CreateST(string blockName, string STCode)
        {
            XmlNode body = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:body", _nsManager);
            body.RemoveAll();
            XmlNode ST = _doc.CreateNode(XmlNodeType.Element, "ST", _nsManager.LookupNamespace("ns1"));
            XmlNode codeContainer = _doc.CreateNode(XmlNodeType.Element, "xhtml", _nsManager.LookupNamespace("xhtml"));
            XmlNode Code = _doc.CreateNode(XmlNodeType.Text,"#text", _nsManager.LookupNamespace("xhtml"));
            Code.Value = STCode;
            codeContainer.AppendChild(Code);
            ST.AppendChild(codeContainer);
            body.AppendChild(ST);
        }
        public void SaveDoc(string fileName)
        {
            _doc.Save(fileName);
        }
        private XmlNode buildVariableNode(string varName, string varType)
        {
            if (_elementaryTypes.Particle.Items.OfType<XmlSchemaElement>().Any(x => x.Name.ToLower() == varType.ToLower()))
            {
                System.Xml.XmlNode variable = _doc.CreateNode(System.Xml.XmlNodeType.Element, "variable", _nsManager.LookupNamespace("ns1"));
                System.Xml.XmlAttribute varibaleName = _doc.CreateAttribute("name");
                varibaleName.Value = varName;
                variable.Attributes.Append(varibaleName);
                System.Xml.XmlNode type = _doc.CreateNode(System.Xml.XmlNodeType.Element, "type", _nsManager.LookupNamespace("ns1"));
                System.Xml.XmlNode dataType = _doc.CreateNode(System.Xml.XmlNodeType.Element, varType.ToUpper(), _nsManager.LookupNamespace("ns1"));
                type.AppendChild(dataType);
                variable.AppendChild(type);
                return variable; 
            }
            return null; 
      
        }
        
        
        private System.Xml.XmlDocument _doc;
        private System.Xml.Schema.XmlSchema _schema;
        private XmlSchemaGroup _elementaryTypes;
        private  System.Xml.XmlNamespaceManager _nsManager;
    }
}