﻿using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;
using Utilities;

namespace ActiveDirectoryManagement
{
    [System.Management.Automation.Cmdlet(System.Management.Automation.VerbsCommon.Get, "AdObject")]
    public class Get_AdObject : System.Management.Automation.PSCmdlet
    {
        [System.Management.Automation.Parameter(Position = 0, Mandatory = false)]
        public string Path;

        [System.Management.Automation.ValidateSet("computer", "user", "group", "organizationalunit")]
        [System.Management.Automation.Parameter(Position = 1, Mandatory = false)]
        public string Type;

        [System.Management.Automation.Parameter(Position = 2, Mandatory = false)]
        public string Filter;

        [System.Management.Automation.ValidateSet("Base", "OneLevel", "Subtree")]
        [System.Management.Automation.Parameter(Position = 3, Mandatory = false)]
        public string Scope;

        [System.Management.Automation.Parameter(Position = 4, Mandatory = false)]
        public string[] Properties;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Path == null)
            {
                WriteVerbose("No Path set on cmdline, setting Path to current domain");
                DirectoryEntry myDir = new DirectoryEntry("LDAP://RootDSE");
                object propertyValue = myDir.Properties["defaultNamingContext"].Value;
                Path = propertyValue.ToString();

                WriteDebug("Close directory entry object");
                myDir.Dispose();
            }

            if (!(Path.ToUpper().Contains("LDAP://")))
            {
                WriteVerbose("LDAP:// not found, appending LDAP:// to Path : " + Path);
                Path = "LDAP://" + Path;
            }

            if (Type == null)
            {
                WriteVerbose("No Type set on cmdline, setting Type to computer");
                Type = "computer";
            }

            if (Filter == null)
            {
                WriteVerbose("No Filter set on cmdline, using the value of Type to build filter");
                switch (Type)
                {
                    case "computer":
                        {
                            WriteVerbose("Using generic LDAP search for " + Type);
                            Filter = "(objectCategory=computer)";
                            break;
                        }
                    case "user":
                        {
                            WriteVerbose("Using generic LDAP search for " + Type);
                            Filter = "(objectCategory=user)";
                            break;
                        }
                    case "group":
                        {
                            WriteVerbose("Using generic LDAP search for " + Type);
                            Filter = "(objectCategory=group)";
                            break;
                        }
                    case "organizationalunit":
                        {
                            WriteVerbose("Using generic LDAP search for " + Type);
                            Filter = "(objectCategory=organizationalunit)";
                            break;
                        }
                    default:
                        {
                            break;
                        }
                } // End Switch
            } // End If

            if (Scope == null)
            {
                WriteVerbose("No Scope set on cmdline, setting Scope to Subtree");
                Scope = "Subtree";
            }

            if (Path.ToUpper().Contains("CN="))
            {
                WriteVerbose("CN Found, searching for a specific item in the directory");
                Filter = "";
                Scope = "Base";
            }

        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            foreach (SearchResult AdObject in (Utilities.Functions.QueryAD(Path, Filter, Scope, Properties)))
            {
                WriteVerbose("Create PowerShell object to hold return values");
                PSObject objReturn = new PSObject();

                WriteVerbose("Add AdObject.Properties to PowerShell object");
                foreach (string AdProperty in AdObject.Properties.PropertyNames)
                {
                    WriteDebug("Add property : " + AdProperty);
                    objReturn.Properties.Add(new PSNoteProperty(AdProperty, (AdObject.Properties[AdProperty])[0]));
                }
                WriteObject(objReturn);
            }
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }

    [System.Management.Automation.Cmdlet(System.Management.Automation.VerbsCommon.Get, "AdGroupMember")]
    public class Get_AdGroupMember : System.Management.Automation.PSCmdlet
    {
        [System.Management.Automation.Parameter(Position = 0, Mandatory = false)]
        public string Path;

        [System.Management.Automation.Parameter(Position = 1, Mandatory = false)]
        public string Name;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (Path == null)
            {
                WriteVerbose("No Path set on cmdline, setting Path to current domain");
                DirectoryEntry myDir = new DirectoryEntry("LDAP://RootDSE");
                object propertyValue = myDir.Properties["defaultNamingContext"].Value;
                Path = propertyValue.ToString();

                WriteDebug("Close directory entry object");
                myDir.Dispose();
            }

            if (!(Path.ToUpper().Contains("LDAP://")))
            {
                WriteVerbose("LDAP:// not found, appending LDAP:// to Path : " + Path);
                Path = "LDAP://" + Path;
            }

        }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            string Filter = "(&(objectCategory=group)(name=" + Name + "))";
            string Scope = "Subtree";
            string[] Properties = null;

            SearchResultCollection AdGroupObject = Utilities.Functions.QueryAD(Path, Filter, Scope, Properties);
            DirectoryEntry Group = AdGroupObject[0].GetDirectoryEntry();
            foreach (string Member in Group.Properties["member"])
            {
                foreach (SearchResult AdObject in (Utilities.Functions.QueryAD("LDAP://" + Member, "", "Base", null)))
                {
                    WriteVerbose("Create PowerShell object to hold return values");
                    PSObject objReturn = new PSObject();
                    WriteVerbose("Add AdObject.Properties to PowerShell object");
                    foreach (string AdProperty in AdObject.Properties.PropertyNames)
                    {
                        WriteDebug("Add property : " + AdProperty);
                        objReturn.Properties.Add(new PSNoteProperty(AdProperty, (AdObject.Properties[AdProperty])[0]));
                    }
                    WriteObject(objReturn);
                }
            }
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
