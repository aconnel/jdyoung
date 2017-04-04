This project is a template for pulling a user from a process field in the Square 9 workflow engine, 
checking various groups to see if that user is a member, then passing a value back to the Return
process field in the Square 9 workflow.  The DLL is called by a Call Assembly node in the workflow.

This project appends the PropertyMapping.xml file from the Square 9 github project SQLSelectQuery
with an additional value named "TheUserName", with dictionary ID 4.  This allows the project to utilize
the already-existing "Return" field (3) so it can work in conjunction with the SQLSelectQuery project.

The XML file can be modified to remove the SQL-related items so this can be used alone, but the
recommended implementation would be to use it as-is, allowing both to be used together.  The XML file
could be separated by changing PropertyMapping.xml in the code to another file and creating that file
with the appropriate fields.  That raises the question of whether you share the "Return" process field
or create a new one just for the purpose of this project.  

I recommend using one PropertyMapping.xml file for these and other projects, and creating a template
workflow with the necessary process fields already populated in it.  This will ensure the process
fields are always in the correct order/position and that they do not interfere with each other.

---------------

Version 1.0.0.0 - First build, updates to come.

REQUIREMENTS:
 - Project is a customizable template.  Necessary edits to the source are:
     * Domain (line 36)
     * AD groups to check (starting line 47)
     * Values to pass to the Return process field when the user is found (starting line 58)
     * Number of characters to be removed from the beginning of the user string to strip the domain (line 41).
       Eg: value is DOMAIN\jsmith.  DOMAIN\ is 7 chars.  userToCheck = userToCheck.Remove(0,7);
 - PropertyMapping.xml must be included in the same directory as the dll. Dictionary IDs for "Return" and
   "TheUserName" MUST match the order/position/ID of the process fields in the workflow.
 - Relies on System.DirectoryServices.AccountManagement namespace, which requires .NET 3.5 or higher.
 - This project assumes the user is only a member of one group.  If they are a member of multiple, the 
   current version will only return a value for the first membership it finds, based on how you order them.
   A need for returning multiple group memberships will require heavy customization.

OPTIONAL:
 - The .xml file name can be customized (line 26) if needed/desired to conform to a naming convention,
   or to split this away from another project such as SQLSelectQuery to keep them completely separate.
 - Value to be returned upon failure to find the user in any group can be customized on line 74.
 - Value to be returned if the user string is null can be customized on line 79.
 - Error log file name can be cuztomized on line 91.

BUGS:
 - None known

FUTURE IMPROVEMENTS / TO-DO

Minor:
 - Change method of removing domain from userToCheck from a character count to something that looks for the 
   "\" and removes that character and everything in front of it.
 - Use the stripped domain from userToCheck to populate the domain on line 36, reducing necessary customizations.
 - Clean up code to remove unnecessary references or other items.

Major
 - Change method of finding the user's group by creating an array from their group memberships, then iterating
   through the array and passing a return value when one is found.
 - Potentially a branch project for returning multiple group values.