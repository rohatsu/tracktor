// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using tracktor.model;

namespace tracktor.service
{
    public interface ITracktorService
    {
        int CreateUser(string userName);

        TSummaryModelDto GetSummaryModel(TContextDto context);

        TStatusModelDto GetStatusModel(TContextDto context);

        TEntriesModelDto GetEntriesModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int startNo, int maxEntries);

        TReportModelDto GetReportModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int taskID);

        TTaskDto UpdateTask(TContextDto context, TTaskDto task);

        TProjectDto UpdateProject(TContextDto context, TProjectDto project);

        TEntryDto GetEntry(TContextDto context, int entryID);

        TEntryDto UpdateEntry(TContextDto context, TEntryDto entry);

        void StopTask(TContextDto context, int currentTaskID);

        void StartTask(TContextDto context, int newTaskID);

        void SwitchTask(TContextDto context, int currentTaskID, int newTaskId);
    }
}
