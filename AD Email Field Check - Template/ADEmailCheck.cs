using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ADEmailCheck
{
    public class ADEmailCheck
    {
        //The Assembly that is referenced from the "Call Assembly" Workflow Node must have a method called "RunCallAssembly."
        //The method must accept and return a Dictionary<string, string> Object.
        //The Dictionary Key is the Workflow Property ID and the Value is the Workflow Property Value.
        public Dictionary<string, string> RunCallAssembly(Dictionary<string, string> Input)
        {
            //Declare the Output variable that will be used to collect/return our processed data.
            Dictionary<string, string> Output = new Dictionary<string, string>();

            try
            {
                //property names and their ids are stored in an xml file. the name is the element, the id has to match the dictionary key (numerical order/position in the process fields list).
                String mappingfile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location), "PropertyMapping.xml");

                // Check to make sure the XML file exists
                if (File.Exists(mappingfile))
                {
                    //we create an object to deserialize the xml to. 
                    PropertyMapping propertyMap = new PropertyMapping();
                    propertyMap = propertyMap.Deserialize(mappingfile);

                    // Set up domain context.  Enter the local domain within the quotes without the extension, such as .local
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "DOMAIN");

                    // Pull username from property map in format domain\username and assign it to a string
                    String userToCheck = Input[propertyMap.TheUserName];

                    // Remove domainname\ from the string.  0 starts at the beginning, 4 represents the number of characters to delete from the front of the string
                    userToCheck = userToCheck.Remove(0, 4);

                    // Set the user to be queried as the username from the string we just created
                    UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userToCheck);

                    // Check to make sure the user to query actually has a value in it.
                    if (user != null)
                    {
                        string email = user.EmailAddress; //Create a string and populate is with the value of the AD email field
                        if (email != null)
                        {
                            Output.Add(propertyMap.ReturnValue, email); //If the value wasn't empty, write it to the ReturnValue variable to be passed to the workflow Return field
                        }
                        else
                        {
                            Output.Add(propertyMap.ReturnValue, "EMAIL_WAS_NULL"); //If the email field was blank, write an error to be passed to the workflow's Return field
                        }
                    }
                    else
                    {
                        Output.Add(propertyMap.ReturnValue, "USER_WAS_NULL");  // Set message to be passed to the workflow's Return field in case the user was null
                    }
                }
                else
                {
                    Output.Add("-1", "PropertyMapping.xml not found.");
                }

            }
            catch (Exception ex)
            {
                //Log some errors out
                String errorPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location), "ADEmailCheck.log");
                File.AppendAllText(errorPath, "\r\n" + DateTime.Now.ToString() + ex.Message + "\r\n" + ex.StackTrace);
            }
            //Finally, return our Output Dictionary Object that will be used set the new Values of each Workflow Property.
            //It is only necessary to return the Property ID's and Values of the Properties that are updated.
            return Output;
        }

        private String giveMeSpace(String path)
        {
            return path.Replace("%20", " ");
        }
    }

    /// <summary>
    /// create an object that will match the xml file
    /// </summary>
    public class PropertyMapping
    {
        // Create a string for each value we want to pull from the XML file.
        public String TheUserName { get; set; }
        public String ReturnValue { get; set; }

        public void Serialize(String file, PropertyMapping propertyMap)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(propertyMap.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, propertyMap);
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }

        public PropertyMapping Deserialize(string file)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(
                  typeof(PropertyMapping));
            StreamReader reader = File.OpenText(file);
            PropertyMapping propertyMap = (PropertyMapping)xs.Deserialize(reader);
            reader.Close();
            reader.Dispose();
            return propertyMap;
        }
    }
}