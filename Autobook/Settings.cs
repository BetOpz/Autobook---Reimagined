using System;
using System.Collections.Generic;
using System.Text;

namespace Autobook
{
  using System;
  using System.Windows.Forms;
  using System.Xml;

  public class Settings
  {
		private const string DefaultFilename = "settings.xml";

    XmlDocument xmlDocument = new XmlDocument ();
		string documentPath;

    public Settings ()
    {
      try
      {
				SetDefaultFile ();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine ("Settings: " + ex.ToString ());
        System.Diagnostics.Trace.WriteLine ("Settings: using defaults");
        xmlDocument.LoadXml ("<settings></settings>");
      }
    }

    public int GetSetting (string xPath, int defaultValue)
    {
      return Convert.ToInt32 (GetSetting (xPath, Convert.ToString (defaultValue)));
    }

    public double GetSetting (string xPath, double defaultValue)
    {
      return Convert.ToDouble (GetSetting (xPath, Convert.ToString (defaultValue)));
    }

    public bool GetSetting (string xPath, bool defaultValue)
    {
      return Convert.ToBoolean (GetSetting (xPath, Convert.ToString (defaultValue)));
    }

    public void PutSetting (string xPath, int value)
    {
      PutSetting (xPath, Convert.ToString (value));
    }

    public void PutSetting (string xPath, double value)
    {
      PutSetting (xPath, Convert.ToString (value));
    }

    public void PutSetting (string xPath, bool value)
    {
      PutSetting (xPath, Convert.ToString (value));
    }

    public string GetSetting (string xPath, string defaultValue)
    {
      XmlNode xmlNode = xmlDocument.SelectSingleNode ("settings/" + xPath);
      if (xmlNode != null) { return xmlNode.InnerText; }
      else { return defaultValue; }
    }

    public void PutSetting (string xPath, string value)
    {
      XmlNode xmlNode = xmlDocument.SelectSingleNode ("settings/" + xPath);
      if (xmlNode == null) { xmlNode = createMissingNode ("settings/" + xPath); }
      xmlNode.InnerText = value;
      xmlDocument.Save (documentPath);
    }

		public string Filename
		{
			get { return documentPath; }
		}

		public void SetDefaultFile ()
		{
			documentPath = Application.StartupPath + "\\" + DefaultFilename;
			xmlDocument.Load (documentPath);
		}

		public string LoadXml (string filename)
		{
			try
			{
				documentPath = filename;
				xmlDocument.Load (documentPath);
				return string.Empty;
			}
			catch (Exception ex)
			{
				//System.Diagnostics.Trace.WriteLine ("Settings: " + ex.ToString ());
				//System.Diagnostics.Trace.WriteLine ("Settings: using defaults");
				xmlDocument.LoadXml ("<settings></settings>");
				return ex.ToString ();
			}
		}

    private XmlNode createMissingNode (string xPath)
    {
      string[] xPathSections = xPath.Split ('/');
      string currentXPath = "";
      XmlNode testNode = null;
      XmlNode currentNode = xmlDocument.SelectSingleNode ("settings");
      foreach (string xPathSection in xPathSections)
      {
        currentXPath += xPathSection;
        testNode = xmlDocument.SelectSingleNode (currentXPath);
        if (testNode == null)
        {
          currentNode.InnerXml += "<" +
                      xPathSection + "></" +
                      xPathSection + ">";
        }
        currentNode = xmlDocument.SelectSingleNode (currentXPath);
        currentXPath += "/";
      }
      return currentNode;
    }
  }
}
