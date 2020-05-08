using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace PerfView {
    public class PerfViewAzureConfiguration : PerfViewDirectory
    {
        public PerfViewAzureConfiguration(string path) : base($"{path}\\azure.config")
        {
        }

        public override IList<PerfViewTreeItem> Children
        {
            get
            {
                if (m_Children == null)
                {
                    m_Children = new List<PerfViewTreeItem>();

                    try
                    {
                        if (!File.Exists(m_filePath)) {
                            using (var writeStream = XmlWriter.Create(File.Create(m_filePath))) {
                                writeStream.WriteStartElement("Connections");
                                
                                writeStream.WriteStartElement("Connection");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.NameAttribute, "SPEWebsite");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.AppIdAttribute, "3d4a6115-b53b-4f61-a6e9-2e3a90e0c15c");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.SecretAttribute, "35dg2cgbe3omxgfyixj2ifcjpqqnjqf8h5e7jjlo");
                                writeStream.WriteEndElement();

                                writeStream.WriteStartElement("Connection");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.NameAttribute, "Test1");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.AppIdAttribute, "ea45bcf3-641c-4d78-87ae-894252939cc2");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.SecretAttribute, "lblwyeodiehgliyel6a01w6ohlthhdt8wieyvqff");
                                writeStream.WriteEndElement();

                                writeStream.WriteStartElement("Connection");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.NameAttribute, "Test2");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.AppIdAttribute, "0393c6f6-7a38-442d-9e84-358c60b92ee5");
                                writeStream.WriteAttributeString(PerfViewAzurePerfTreeGroup.SecretAttribute, "cxwak8cs3euz2mcwne2jtpwwqecm68rwwya0pekg");
                                writeStream.WriteEndElement();

                                writeStream.WriteEndElement();

                                writeStream.Close();
                            }
                        }

                        using (var xmlReader = XmlReader.Create(m_filePath)) {
                            xmlReader.Read();
                            if (xmlReader.Read() && xmlReader.Name == "Connections") {
                                while (xmlReader.Read() && xmlReader.IsEmptyElement && xmlReader.Name == "Connection") {
                                    m_Children.Add(new PerfViewAzurePerfTreeGroup(xmlReader));
                                }
                            }
                        }
                    }
                    // FIX NOW review
                    catch (Exception) { }
                }
                return m_Children;
            }
        }
        public override string HelpAnchor { get { return null; } }      // Don't bother with help for this.  

        /// <summary>
        /// Open the file (This might be expensive (but maybe not).  This should populate the Children property 
        /// too.  
        /// </summary>
        public override void Open(Window parentWindow, StatusBar worker, Action doAfter)
        {
            doAfter?.Invoke();
        }
        /// <summary>
        /// Close the file
        /// </summary>
        public override void Close() { }

        public override ImageSource Icon { get { return GuiApp.MainWindow.Resources["FolderOpenBitmapImage"] as ImageSource; } }

        //#region private

        //private class DirCacheEntry
        //{
        //    public string[] FilesInDirectory;
        //    public string[] DirsInDirectory;
        //    public DateTime LastWriteTimeUtc;
        //}

        //// To speed things up we remember the list list of directory items we fetched from disk
        //private static Dictionary<string, DirCacheEntry> s_dirCache = new Dictionary<string, DirCacheEntry>();

        //private static string[] FilesInDirectory(string directoryPath)
        //{
        //    var entry = GetDirEntry(directoryPath);
        //    if (entry.FilesInDirectory == null)
        //    {
        //        entry.FilesInDirectory = Directory.GetFiles(directoryPath);
        //    }

        //    return entry.FilesInDirectory;
        //}

        //private static string[] DirsInDirectory(string directoryPath)
        //{
        //    var entry = GetDirEntry(directoryPath);
        //    if (entry.DirsInDirectory == null)
        //    {
        //        entry.DirsInDirectory = Directory.GetDirectories(directoryPath);
        //    }

        //    return entry.DirsInDirectory;
        //}

        ///// <summary>
        ///// Gets a cache entry, nulls it out if it is out of date.  
        ///// </summary>
        //private static DirCacheEntry GetDirEntry(string directoryPath)
        //{
        //    DateTime lastWrite = Directory.GetLastWriteTimeUtc(directoryPath);
        //    DirCacheEntry entry;
        //    if (!s_dirCache.TryGetValue(directoryPath, out entry))
        //    {
        //        s_dirCache[directoryPath] = entry = new DirCacheEntry();
        //    }

        //    if (lastWrite != entry.LastWriteTimeUtc)
        //    {
        //        entry.DirsInDirectory = null;
        //        entry.FilesInDirectory = null;
        //    }
        //    entry.LastWriteTimeUtc = lastWrite;
        //    return entry;
        //}

        //private Regex m_filter;
        //#endregion
    }

    /// <summary>
    /// A PerfViewTreeGroup simply groups other Items.  Thus it has a name, and you use the Children
    /// to add Child nodes to the group.  
    /// </summary>
    public class PerfViewAzurePerfTreeGroup : PerfViewTreeItem {
        public const string NameAttribute = "name";
        public const string AppIdAttribute = "appId";
        public const string SecretAttribute = "secret";

        public readonly string AppId;

        public readonly string Secret;

        public PerfViewAzurePerfTreeGroup(XmlReader xmlReader) {
            Name = xmlReader.GetAttribute(NameAttribute);
            AppId = xmlReader.GetAttribute(AppIdAttribute);
            Secret = xmlReader.GetAttribute(SecretAttribute);
            m_Children = new List<PerfViewTreeItem>();
        }

        // Groups do no semantic action.   All the work is in the visual GUI part.  
        public override void Open(Window parentWindow, StatusBar worker, Action doAfter = null) {
            // TODO: Open Azure connection and read in perf tree.

            doAfter?.Invoke();
        }
        public override void Close() { }

        private const string queryUri = "https://api.applicationinsights.io/v1/apps/{0}/query";
        private static HttpClient httpClient = new HttpClient();

        public override IList<PerfViewTreeItem> Children {
            get {
                try {
                    var startTime = "2020-04-15T02:08:00.000Z";
                    var endTime = "2020-04-22T03:33:00.000Z";
                    var query = $"set truncationmaxrecords=10000;set truncationmaxsize=67108864;let numPoints = 100;  let profilerSamples = customEvents | where timestamp > datetime(\"{startTime}\") and timestamp < datetime(\"{endTime}\")  | where name == \"ServiceProfilerSample\" and isnotempty(customDimensions.ServiceProfilerContent)  | project tostring(customDimensions.ServiceProfilerContent), requestId = tostring(customDimensions.RequestId);  let requestsBase = requests | where timestamp > datetime(\"{startTime}\") and timestamp < datetime(\"{endTime}\") | where client_Type != \"Browser\"   | extend id = pack_array(id, strcat(\" |\", operation_Id, \".\", id, \".\")) | mvexpand id  | project requestId=tostring(id), itemCount, itemId, operation_Id, operation_Name, duration, timestamp, cloud_RoleInstance, cloud_RoleName, client_CountryOrRegion, customDimensions, resultCode, success, url;  let subRequests = profilerSamples | join kind = inner requestsBase on requestId  | extend hasProfiler = iff(isnotempty(customDimensions_ServiceProfilerContent), true, false);  let maxDuration=toscalar(subRequests   | summarize max(duration));  let xBucketBase = pow(maxDuration, 1/todouble(numPoints));  let bucketSize=maxDuration / numPoints;  let xBucketTable=range bucketSlot from 0 to numPoints - 1 step 1;  xBucketTable  | join kind=leftouter(  subRequests  | extend adjustedDuration = iff(duration<1, 1.0, duration)  | extend bucketSlot = iff(tolong(bin(log(adjustedDuration)/log(xBucketBase), 1)) > numPoints - 1, numPoints - 1,   tolong(bin(log(adjustedDuration)/log(xBucketBase), 1)))  | summarize sum(itemCount), profileCount=countif(hasProfiler), any(*) by bucketSlot, hasProfiler) on bucketSlot  | summarize [\"count\"] = sum(iff(isnull(sum_itemCount), 0, sum_itemCount)), profileCount = sum(profileCount),   any(hasProfiler, *) by bucketSlot  | project bucketSlot, bucketStart=pow(xBucketBase, bucketSlot), [\"count\"], profileCount,  profilerId=any_any_customDimensions_ServiceProfilerContent, itemId=any_any_itemId, operationId=any_any_operation_Id, operationName=any_any_operation_Name, duration=any_any_duration, timestamp=any_any_timestamp, roleInstance=any_any_cloud_RoleInstance, roleName=any_any_cloud_RoleName, region=any_any_client_CountryOrRegion, customDimensions=any_any_customDimensions, resultCode=any_any_resultCode, success=any_any_success, url=any_any_url | order by bucketSlot asc";
                    var timespan = $"{startTime}/{endTime}";
                    var json = $"{{ \"cls\": \"{query.Replace("\"", "\\\"")}\", \"timespan\": \"{timespan}\" }}";

                    using (var jsonContent = new StringContent(json)) {
                        jsonContent.Headers.Add("X-Api-Key", Secret);
                        jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        var response = httpClient.PostAsync(string.Format(queryUri, AppId), jsonContent).Result;
                        var body = response.Content.ReadAsStringAsync().Result;
                    }
                } catch (Exception e) {
                    //StatusBar.log
                }

                return m_Children;
            }
        }

        public override string HelpAnchor { get { return Name.Replace(" ", ""); } }

        public override ImageSource Icon { get { return GuiApp.MainWindow.Resources["FolderOpenBitmapImage"] as ImageSource; } }
    }

    /// <summary>
    /// A PerfViewTreeGroup simply groups other Items.  Thus it has a name, and you use the Children
    /// to add Child nodes to the group.  
    /// </summary>
    public class PerfViewAzurePerfInstanceTreeGroup : PerfViewTreeItem {
        public const string NameAttribute = "name";
        public const string ConnectionAttribute = "connection";
        public const string SecretAttribute = "secret";

        public readonly string Connection;

        public readonly string Secret;

        public PerfViewAzurePerfInstanceTreeGroup(XmlNode connectionNode) {
            Name = connectionNode.Attributes[NameAttribute].InnerText;
            Connection = connectionNode.Attributes[ConnectionAttribute].InnerText;
            Secret = connectionNode.Attributes[SecretAttribute].InnerText;
            m_Children = new List<PerfViewTreeItem>();
        }

        // Groups do no semantic action.   All the work is in the visual GUI part.  
        public override void Open(Window parentWindow, StatusBar worker, Action doAfter = null) {
            // TODO: Open Azure connection and read in perf tree.

            doAfter?.Invoke();
        }
        public override void Close() { }

        public override IList<PerfViewTreeItem> Children { get { return m_Children; } }

        public override string HelpAnchor { get { return Name.Replace(" ", ""); } }

        public override ImageSource Icon { get { return GuiApp.MainWindow.Resources["FolderOpenBitmapImage"] as ImageSource; } }
    }

    // Used for new user defined file formats.  
    internal class PerfViewAzurePerfFileTreeGroup : PerfViewFile {
        public PerfViewAzurePerfFileTreeGroup(string formatName, string[] fileExtensions) {
            m_formatName = formatName;
            m_fileExtensions = fileExtensions;
        }

        public override string FormatName { get { return m_formatName; } }
        public override string[] FileExtensions { get { return m_fileExtensions; } }
        protected override Action<Action> OpenImpl(Window parentWindow, StatusBar worker) {
            return null;
        }

        #region private
        private string m_formatName;
        private string[] m_fileExtensions;
        #endregion
    }
}
