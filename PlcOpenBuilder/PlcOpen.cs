using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace PlcOpenBuilder
{
    public class PlcOpen
    {
        /// <summary>
        /// Constructor used to edit existing XML
        /// </summary>
        /// <param name="fileName"> File path to existing XML</param>
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
        /// <summary>
        /// Constructor to create new Xml
        /// </summary>
        /// <param name="companyName">Description, IE Bosch-Rexroth</param>
        /// <param name="productName">Destription, IE Ctrlx Core</param>
        /// <param name="productVersion">Descrition, IE 1.0</param>
        /// <param name="projectName">Description, IE CodeGenerator</param>
        public PlcOpen(string companyName, string productName, string productVersion, string projectName) {
            //Builds out the XML Header data for PLC Open
            _doc = new XmlDocument();
            XmlProcessingInstruction xmlInstructions = _doc.CreateProcessingInstruction("xml", "version=\"1.0\"  encoding=\"utf-8\"");
            _doc.AppendChild(xmlInstructions);
            _nsManager = new System.Xml.XmlNamespaceManager(_doc.NameTable);
            _nsManager.AddNamespace("ns1", "http://www.plcopen.org/xml/tc6_0200");
            _nsManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
            XmlNode Project = _doc.CreateNode(XmlNodeType.Element, "project", _nsManager.LookupNamespace("ns1"));
            XmlNode Header = _doc.CreateNode(XmlNodeType.Element, "fileHeader", _nsManager.LookupNamespace("ns1"));
            XmlAttribute company = _doc.CreateAttribute("companyName"); 
            company.Value = companyName;
            XmlAttribute prodName = _doc.CreateAttribute("productName");
            prodName.Value = productName;
            XmlAttribute version = _doc.CreateAttribute("productVersion");
            version.Value = productVersion;
            XmlAttribute createtionDate = _doc.CreateAttribute("creationDateTime");
            createtionDate.Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
            Header.Attributes.Append(company); 
            Header.Attributes.Append(prodName);
            Header.Attributes.Append(version);
            Header.Attributes.Append(createtionDate); 
            Project.AppendChild(Header);
           
            XmlNode contentHeader = _doc.CreateNode(XmlNodeType.Element, "contentHeader", _nsManager.LookupNamespace("ns1"));
            XmlAttribute projName = _doc.CreateAttribute("name");
            projName.Value = projectName;
            contentHeader.Attributes.Append(projName);
            XmlAttribute modificationTime = _doc.CreateAttribute("modificationDateTime");
            modificationTime.Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"); 
            contentHeader.Attributes.Append(modificationTime);
            XmlNode coordinateInfo = _doc.CreateNode(XmlNodeType.Element, "coordinateInfo", _nsManager.LookupNamespace("ns1"));

            XmlNode fbd = _doc.CreateNode(XmlNodeType.Element, "fbd", _nsManager.LookupNamespace("ns1"));
            XmlNode fbdScaling = _doc.CreateNode(XmlNodeType.Element, "scaling", _nsManager.LookupNamespace("ns1")); 
            XmlAttribute x = _doc.CreateAttribute("x");
            x.Value = "1"; 
            XmlAttribute y = _doc.CreateAttribute("y");
            y.Value = "1";
            fbdScaling.Attributes.Append(x);
            fbdScaling.Attributes.Append(y);
            fbd.AppendChild(fbdScaling); 

            XmlNode ld = _doc.CreateNode(XmlNodeType.Element, "ld", _nsManager.LookupNamespace("ns1"));
            ld.AppendChild(fbdScaling.Clone()); 

            XmlNode sfc = _doc.CreateNode(XmlNodeType.Element, "sfc", _nsManager.LookupNamespace("ns1"));
            sfc.AppendChild(fbdScaling.Clone());

            coordinateInfo.AppendChild(fbd);
            coordinateInfo.AppendChild(ld);
            coordinateInfo.AppendChild(sfc);
            contentHeader.AppendChild(coordinateInfo);

            Project.AppendChild(contentHeader);
            XmlNode types = _doc.CreateNode(XmlNodeType.Element, "types", _nsManager.LookupNamespace("ns1"));
            XmlNode dataTypes = _doc.CreateNode(XmlNodeType.Element, "dataTypes", _nsManager.LookupNamespace("ns1"));
            XmlNode pous = _doc.CreateNode(XmlNodeType.Element, "pous", _nsManager.LookupNamespace("ns1")); 
            types.AppendChild(dataTypes);
            types.AppendChild(pous);
            Project.AppendChild(types);

            XmlNode instances = _doc.CreateNode(XmlNodeType.Element, "instances", _nsManager.LookupNamespace("ns1"));
            XmlNode config = _doc.CreateNode(XmlNodeType.Element, "configurations", _nsManager.LookupNamespace("ns1"));
            instances.AppendChild(config);

            Project.AppendChild(instances); 
            _doc.AppendChild(Project);
            TextReader stream = new StringReader(Properties.Resources.tc6_xml_v201);
            _schema = XmlSchema.Read(stream, validateScheme);
            _elementaryTypes = _schema.Items.OfType<XmlSchemaGroup>().Where(x => x.Name == "elementaryTypes").First();
        }

        private void validateScheme(object? sender, ValidationEventArgs e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Adds an input to a POU
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddInput(string blockName,string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:inputVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));

        }
        /// <summary>
        /// Adds an output to a POU
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddOutput(string blockName, string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:outputVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));
        }
        /// <summary>
        /// Adds an inOutput to a POU
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddInOut(string blockName, string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:inOutVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));
        }
        /// <summary>
        /// Adds an temp to a POU
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddTemp(string blockName, string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:tempVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));
        }
        /// <summary>
        /// Adds a local var to the POU
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddVar(string blockName, string varName, string varType)
        {
            System.Xml.XmlNode inputVars = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:localVars", _nsManager);
            inputVars.AppendChild(buildVariableNode(varName, varType));
        }
        /// <summary>
        /// Adds a local constant var to pou
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddConstVar(string blockName, string varName, string varType)
        {
            System.Xml.XmlNodeList nodes = _doc.DocumentElement.SelectNodes("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:localVars", _nsManager);
            XmlNode constant = null;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name == "constant")
                        {
                            constant = node;
                            break; 
                        }
                    }
                
                }
            }
            if (constant != null)
                constant.AppendChild(buildVariableNode(varName, varType));
        }
        /// <summary>
        /// Adds a local persistant var to pou
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="varType">Type of Variable, can be a UDT/FB</param>
        public void AddPersistentVar(string blockName, string varName, string varType)
        {
            System.Xml.XmlNodeList nodes = _doc.DocumentElement.SelectNodes("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface/ns1:localVars", _nsManager);
            XmlNode persistent = null; 
            bool persistFound = false;
            bool retainFound = false; 
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name == "persistent")
                        {
                            persistFound = true;
                        }
                        if(att.Name =="retain")
                        {
                            retainFound = true;
                        }
                        if (persistFound && retainFound)
                        {
                            persistent = node;
                            break; 
                        }
                       
                    }
                    persistFound = false;
                    retainFound = false; 
                }
            }
            if(persistent != null)
                persistent.AppendChild(buildVariableNode(varName, varType));
        }
        /// <summary>
        /// sets the length of a string, IE String(100) 
        /// </summary>
        /// <param name="blockName">Name of POU</param>
        /// <param name="varName">Name of Variable</param>
        /// <param name="length">String representation of the number of chars in the string type, IE 200 is string(200)>
        public void setStringLength(string blockName, string varName, string length)
        {
            System.Xml.XmlNode pouInterface = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface", _nsManager);
            XmlNode Var = null;
            foreach (XmlNode vars in pouInterface.ChildNodes)
            {
                try
                {
                    Var = vars.ChildNodes.OfType<XmlNode>().First(x => (x.Attributes.GetNamedItem("name") != null && x.Attributes.GetNamedItem("name").Value == varName));
                }
                catch (Exception ex)
                {
                    continue;
                }
                if (Var != null) { break; }
            }
            if (Var != null)
            {
                XmlAttribute lengthAtr = _doc.CreateAttribute("length");
                lengthAtr.Value = length;
                XmlNode type = Var.SelectSingleNode("ns1:type/ns1:string", _nsManager);
                if (type != null)
                    type.Attributes.Append(lengthAtr); 
            }
        }
        /// <summary>
        /// Sets the start up value of built in type
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="varName"></param>
        /// <param name="value"></param>
        public void InitialValue(string blockName, string varName, string value)
        {
            System.Xml.XmlNode pouInterface = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface", _nsManager);
            XmlNode Var = null; 
            foreach(XmlNode vars in pouInterface.ChildNodes)
            {
                try
                {
                    Var = vars.ChildNodes.OfType<XmlNode>().First(x => (x.Attributes.GetNamedItem("name") != null && x.Attributes.GetNamedItem("name").Value == varName));
                }
                catch (Exception ex)
                {
                    continue;
                }
                if (Var != null) { break; }
            }
            if (Var != null)
            {
                XmlNode initialValue = _doc.CreateNode(XmlNodeType.Element, "initialValue", _nsManager.LookupNamespace("ns1"));
                XmlNode simpleValue = _doc.CreateNode(XmlNodeType.Element, "simpleValue", _nsManager.LookupNamespace("ns1"));
                XmlAttribute valueAtr = _doc.CreateAttribute("value"); 
                valueAtr.Value = value;
                simpleValue.Attributes.Append(valueAtr); 
                initialValue.AppendChild(simpleValue);
                Var.AppendChild(initialValue);
            }
        }
        /// <summary>
        /// Adds a comment to variable
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="varName"></param>
        /// <param name="value"></param>
        public void VarComment(string blockName, string varName, string value)
        {
            System.Xml.XmlNode pouInterface = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface", _nsManager);
            XmlNode Var = null;
            foreach (XmlNode vars in pouInterface.ChildNodes)
            {
                try
                {
                    Var = vars.ChildNodes.OfType<XmlNode>().First(x => (x.Attributes.GetNamedItem("name") != null && x.Attributes.GetNamedItem("name").Value == varName));
                }
                catch (Exception ex)
                {
                    continue;
                }
                if (Var != null) { break; }
            }
            if (Var != null)
            {
                XmlNode documentation = _doc.CreateNode(XmlNodeType.Element, "documentation", _nsManager.LookupNamespace("ns1"));
                documentation.InnerText = "<xhtml xmlns=\"http://www.w3.org/1999/xhtml\">" + value + "</xhtml>"; 
                Var.AppendChild(documentation);
            }
        }
        /// <summary>
        /// stes the start up values for a UDT 
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="varName"></param>
        /// <param name="values"></param>
        public void InitialValue(string blockName, string varName,UDT values)
        {
            System.Xml.XmlNode pouInterface = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous/ns1:pou[@name=\'" + blockName + "\']/ns1:interface", _nsManager);
            XmlNode Var = null;
            foreach (XmlNode vars in pouInterface.ChildNodes)
            {
                Var = vars.ChildNodes.OfType<XmlNode>().First(x => (x.Attributes.GetNamedItem("name") != null && x.Attributes.GetNamedItem("name").Value == varName));
                if (Var != null) { break; }
            }
            if (Var != null)
            {
                XmlNode initialValue = _doc.CreateNode(XmlNodeType.Element, "initialValue", _nsManager.LookupNamespace("ns1"));
                XmlNode structValue = _doc.CreateNode(XmlNodeType.Element, "structValue", _nsManager.LookupNamespace("ns1"));
                foreach (UDTMemeber member in values.Memebers)
                {
                    XmlNode valueNode = _doc.CreateNode(XmlNodeType.Element, "value", _nsManager.LookupNamespace("ns1")); 
                    XmlAttribute memberName = _doc.CreateAttribute("member");
                    memberName.Value = member.Name;
                    valueNode.Attributes.Append(memberName); 
                    XmlNode simpleValue = _doc.CreateNode(XmlNodeType.Element, "simpleValue", _nsManager.LookupNamespace("ns1"));
                    XmlAttribute valueAtr = _doc.CreateAttribute("value");
                    valueAtr.Value = member.Value;
                    simpleValue.Attributes.Append(valueAtr);
                    valueNode.AppendChild(simpleValue);
                    structValue.AppendChild(valueNode);
                   
                }
                initialValue.AppendChild(structValue);
                Var.AppendChild(initialValue);
            }
        }
        /// <summary>
        /// Adds a POU
        /// </summary>
        /// <param name="pouName">Name of POU</param>
        /// <param name="type">Type of POU</param>
        public void AddPou(string pouName, POUType type)
        {
            //Builds a POU with all possible variable interfaces
            XmlNode pous = _doc.DocumentElement.SelectSingleNode("ns1:types/ns1:pous", _nsManager);
            XmlNode pou = _doc.CreateNode(XmlNodeType.Element,"pou", _nsManager.LookupNamespace("ns1"));
            XmlAttribute name = _doc.CreateAttribute("name"); 
            name.Value = pouName;
            XmlAttribute pouType = _doc.CreateAttribute("pouType");
            pouType.Value = _pouString[type];
            pou.Attributes.Append(name); 
            pou.Attributes.Append(pouType);
            XmlNode blockInterface = _doc.CreateNode(XmlNodeType.Element, "interface", _nsManager.LookupNamespace("ns1")); 
            XmlNode inputVars = _doc.CreateNode(XmlNodeType.Element, "inputVars", _nsManager.LookupNamespace("ns1"));
            XmlNode outputVars = _doc.CreateNode(XmlNodeType.Element, "outputVars", _nsManager.LookupNamespace("ns1"));
            XmlNode inoutVars = _doc.CreateNode(XmlNodeType.Element, "inOutVars", _nsManager.LookupNamespace("ns1"));
            XmlNode localVars = _doc.CreateNode(XmlNodeType.Element, "localVars", _nsManager.LookupNamespace("ns1"));
            XmlNode ConstVars = _doc.CreateNode(XmlNodeType.Element, "localVars", _nsManager.LookupNamespace("ns1"));

            XmlAttribute constant = _doc.CreateAttribute("constant");
            constant.Value = "true";
            ConstVars.Attributes.Append(constant);
            XmlNode PersistantVars = _doc.CreateNode(XmlNodeType.Element, "localVars", _nsManager.LookupNamespace("ns1"));
            XmlAttribute Persist = _doc.CreateAttribute("persistent");
            Persist.Value = "true";
            XmlAttribute retain = _doc.CreateAttribute("retain");
            retain.Value = "true";
            PersistantVars.Attributes.Append(Persist);
            PersistantVars.Attributes.Append(retain);

            XmlNode tempVars = _doc.CreateNode(XmlNodeType.Element, "tempVars", _nsManager.LookupNamespace("ns1"));

            blockInterface.AppendChild(inputVars);
            blockInterface.AppendChild(outputVars);
            blockInterface.AppendChild(inoutVars); 
            blockInterface.AppendChild(localVars);
            blockInterface.AppendChild(PersistantVars);
            blockInterface.AppendChild(ConstVars);
            blockInterface.AppendChild(tempVars);
            pou.AppendChild(blockInterface);
            XmlNode body = _doc.CreateNode(XmlNodeType.Element, "body", _nsManager.LookupNamespace("ns1"));
            pou.AppendChild(body);
            pous.AppendChild(pou); 
        }
       /// <summary>
       /// Takes a string of ST code and puts it into the block
       /// </summary>
       /// <param name="blockName">POU of an ST block</param>
       /// <param name="STCode">The actual ST code represented as a string</param>
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
        /// <summary>
        /// Save the xml to be imported 
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveDoc(string fileName)
        {
            XmlNode contentHeader = _doc.DocumentElement.SelectSingleNode("ns1:contentHeader", _nsManager);
            XmlAttribute UpdateTime = contentHeader.Attributes.OfType<XmlAttribute>().First(x => x.Name == "modificationDateTime"); 
            UpdateTime.Value = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
            _doc.Save(fileName);
        }
    
        private XmlNode buildVariableNode(string varName, string varType)
        {
            System.Xml.XmlNode variable = _doc.CreateNode(System.Xml.XmlNodeType.Element, "variable", _nsManager.LookupNamespace("ns1"));
            System.Xml.XmlAttribute varibaleName = _doc.CreateAttribute("name");
            varibaleName.Value = varName;
            variable.Attributes.Append(varibaleName);
            XmlNode dataType = null; 
            System.Xml.XmlNode type = _doc.CreateNode(System.Xml.XmlNodeType.Element, "type", _nsManager.LookupNamespace("ns1"));
            if (_elementaryTypes.Particle.Items.OfType<XmlSchemaElement>().Any(x => x.Name.ToLower() == varType.ToLower()))
            {
                if (varType.ToLower() == "string")
                {
                    dataType = _doc.CreateNode(System.Xml.XmlNodeType.Element, varType.ToLower(), _nsManager.LookupNamespace("ns1"));
                }
                else
                {
                    dataType = _doc.CreateNode(System.Xml.XmlNodeType.Element, varType.ToUpper(), _nsManager.LookupNamespace("ns1"));
                }
            }
            else
            {
                dataType = _doc.CreateNode(System.Xml.XmlNodeType.Element, "derived", _nsManager.LookupNamespace("ns1"));
                XmlAttribute typename = _doc.CreateAttribute("name");
                typename.Value = varType; 
                dataType.Attributes.Append(typename); 
            }
            type.AppendChild(dataType);
            variable.AppendChild(type);
            return variable; 

        }
        
        
        private System.Xml.XmlDocument _doc;
        private System.Xml.Schema.XmlSchema _schema;
        private XmlSchemaGroup _elementaryTypes;
        private  System.Xml.XmlNamespaceManager _nsManager;
        private Dictionary<POUType, string> _pouString = new Dictionary<POUType, string>() { { POUType.Function, "function" }, { POUType.Program, "program" }, { POUType.Function_Block, "functionBlock" } }; 
    }
}