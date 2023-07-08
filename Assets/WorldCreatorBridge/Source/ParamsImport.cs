// Project: WorldCreatorBridge
// Filename: SettingsImport.cs
// Copyright (c) 2023 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System.Globalization;
using System.Xml;
using UnityEngine;

namespace BtB.WC.Bridge
{
    public class ParamsImport
    {
        public string directoryXml = "";
        public string directoryAssets = "";
        public int wcVersion = 2;
        public string nameEnding = "";
        public XmlNode xmlTexture = null;

        public XmlNode curXmlNode;
        public XmlElement curXmlElement;
        public string curXmlPrefix;
        
        // Max Tiles 
        public int maxTilesX = 1;
        public int maxTilesY = 1;
        
        // Max size of the current tile
        public int tileResX = 4096;
        public int tileResY = 4096;
        
        public int terrainLeftX = 4096;
        public int terrainLeftY = 4096;

        public int curTileX = 0;
        public int curTileY = 0;
        
        // Cur subpart start
        public int curPartX = 0;
        public int curPartY = 0;
        
        public int locationX = 0;
        public int locationY = 0;
        public float precision = 1;

        public float height = 1;

        public void SetTile(int curX, int curY, int locX = 0, int locY = 0)
        {
            curTileX = curX;
            curTileY = curY;
            locationX = locX;
            locationY = locY;

            if (wcVersion == 2)
                nameEnding = "_" + curX + "_" + curY;
            else
                nameEnding = "";
        }

        public void SetXmlTexture(XmlElement splatmap)
        {
            if (wcVersion == 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(directoryXml + "\\Assets\\" + splatmap.Attributes["FileName"].Value + "\\Description.xml");
                curXmlNode = doc.GetElementsByTagName("Textures")[0];
                curXmlPrefix = splatmap.Attributes["FileName"].Value + "\\";
            }
            else
                curXmlElement = splatmap;
        }

        public bool HasTexture(string name)
        {
            if (wcVersion == 0)
            {
                if (name == "AlbedoFile") name = "Diffuse";
                else if (name == "NormalFile") name = "Normal";
                else if (name == "DisplacementFile") name = "Displacement";

                foreach (XmlElement texture in curXmlNode.ChildNodes)
                    if (texture.Name == name) return true;

                return false;
            }
            // not WC2 
            return curXmlElement.HasAttribute(name);
        }

        public string GetTexture(string name)
        {
            if (wcVersion == 0)
            {
                if (name == "AlbedoFile") name = "Diffuse";
                else if (name == "NormalFile") name = "Normal";
                else if (name == "DisplacementFile") name = "Displacement";

                foreach (XmlElement texture in curXmlNode.ChildNodes)
                    if (texture.Name == name)
                        return curXmlPrefix + texture.Attributes["File"].Value;
            }

            // not WC2
            return curXmlElement.Attributes[name].Value;
        }

        public void ComputeHeight(XmlNode xmlSurface)
        {
            // Temp values
            float tMinHeight, tMaxHeight, tHeight;
            if (wcVersion == 0)
            {
                float.TryParse(xmlSurface.Attributes["MinHeight"].Value, out tMinHeight);
                float.TryParse(xmlSurface.Attributes["MaxHeight"].Value, out tMaxHeight);
                float.TryParse(xmlSurface.Attributes["Height"].Value, out tHeight);
                height = (tMaxHeight - tMinHeight) * height;
            }
            else if (wcVersion == 1)
            {
                float.TryParse(xmlSurface.Attributes["MinHeight"].Value, out tMinHeight);
                float.TryParse(xmlSurface.Attributes["MaxHeight"].Value, out tMaxHeight);
                height = tMaxHeight - tMinHeight;
            }
            else
            {
                float.TryParse(xmlSurface.Attributes["MinHeight"].Value, out tMinHeight);
                float.TryParse(xmlSurface.Attributes["MaxHeight"].Value, out tMaxHeight);
                height = .01f * (tMaxHeight - tMinHeight);
            }
        }
    }
}