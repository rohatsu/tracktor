﻿// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using tracktor.model.DAL;

namespace tracktor.service
{
    [DataContract]
    public class TContextDto
    {
        [DataMember]
        public int TUserID { get; set; }
        [DataMember]
        public int UTCOffset { get; set; }
    }

    [DataContract]
    public struct TContribDto
    {
        [DataMember]
        public double Today { get; set; }
        [DataMember]
        public double ThisWeek { get; set; }
        [DataMember]
        public double ThisMonth { get; set; }
    }

    [DataContract]
    public class TSummaryModelDto
    {
        [DataMember]
        public List<TProjectDto> Projects { get; set; }

        [DataMember]
        public bool InProgress { get; set; }
    }

    [DataContract]
    public class TStatusModelDto
    {
        [DataMember]
        public bool InProgress { get; set; }

        [DataMember]
        public TTaskDto TTaskInProgress { get; set; }

        [DataMember]
        public TEntryDto LatestEntry { get; set; }
    }

    [DataContract]
    public class TEditModelDto
    {
        [DataMember]
        public TEntryDto Entry { get; set; }
    }

    [DataContract]
    public class TEntriesModelDto
    {
        [DataMember]
        public List<TEntryDto> Entries { get; set; }
    }

    [DataContract]
    public class TEntryDto
    {
        [DataMember]
        public int TEntryID { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public DateTime? EndDate { get; set; }
        [DataMember]
        public bool InProgress { get; set; }
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public string TaskName { get; set; }
        [DataMember]
        public int TTaskID { get; set; }
        [DataMember]
        public double Contrib { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
    }

    [DataContract]
    public class TTaskDto
    {
        [DataMember]
        public int TTaskID { get; set; }
        [DataMember]
        public int TProjectID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public bool InProgress { get; set; }
        [DataMember]
        public bool IsObsolete { get; set; }
        [DataMember]
        public TContribDto Contrib { get; set; }
    }

    [DataContract]
    public class TProjectDto
    {
        [DataMember]
        public int TProjectID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public bool InProgress { get; set; }
        [DataMember]
        public bool IsObsolete { get; set; }
        [DataMember]
        public List<TTaskDto> TTasks { get; set; }
    }

    [DataContract]
    public class TReportModelDto
    {
        [DataMember]
        public Dictionary<DateTime, double> DayContribs { get; set; }
        [DataMember]
        public Dictionary<int, double> TaskContribs { get; set; }
    }
}
