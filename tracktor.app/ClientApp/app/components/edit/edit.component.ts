import { Component, OnInit, AfterViewChecked } from '@angular/core';
import { AppService } from '../../app.service';
import $ = require('jquery');
import * as moment from 'moment';
import { TaskModel } from '../../app.models';

@Component({
    selector: '[tracktor-edit]',
    templateUrl: './edit.component.html',
})
export class EditComponent implements OnInit, AfterViewChecked {

    public startOptions = {
        format: "DD/MM/YYYY HH:mm:ss",
        sideBySide: true,
        useCurrent: false
    };

    public endOptions = {
        format: "DD/MM/YYYY HH:mm:ss",
        sideBySide: true,
        useCurrent: false
    };

    public tasks: TaskModel[];
    public taskId: number;
    public startDate: moment.Moment;
    public endDate: moment.Moment;

    public changeStart(date: moment.Moment) {
        if (this.appService.editModel) {
            this.appService.editModel.entry.startDate = date.format("YYYY-MM-DDTHH:mm:ss.SSS");
            if (this.appService.editModel.entry.endDate) {
                this.appService.editModel.entry.contrib = (moment(this.appService.editModel.entry.endDate).toDate().valueOf() - moment(this.appService.editModel.entry.startDate).toDate().valueOf()) / 1000;
            }
        }
    }

    public changeEnd(date: moment.Moment) {
        if (this.appService.editModel) {
            this.appService.editModel.entry.endDate = date.format("YYYY-MM-DDTHH:mm:ss.SSS");
            if (this.appService.editModel.entry.endDate) {
                this.appService.editModel.entry.contrib = (moment(this.appService.editModel.entry.endDate).toDate().valueOf() - moment(this.appService.editModel.entry.startDate).toDate().valueOf()) / 1000;
            }
        }
    }

    ngOnInit() {
        if (this.appService.editModel) {
            this.startDate = moment(this.appService.editModel.entry.startDate);
            this.endDate = moment(this.appService.editModel.entry.endDate);
            this.taskId = this.appService.editModel.entry.tTaskID;

            // lookup project tasks
            if (this.appService.editModel !== undefined && this.appService.summaryModel !== undefined) {
                const editProjectId = this.appService.editModel.entry.tProjectID;
                const foundProject = this.appService.summaryModel.projects.find(p => p.tProjectID === editProjectId);
                if (foundProject !== undefined) {
                    this.tasks = foundProject.tTasks;
                }
            }
        }
    }

    ngAfterViewChecked() {
        (<any>$("#entryEditModal")).modal();
    }

    public close() {
        (<any>$("#entryEditModal")).modal('hide');
        this.appService.editDialogShown = false;
    }

    public save() {
        if (this.appService.editModel) {
            this.appService.saveEntry(this.appService.editModel.entry.tEntryID, this.appService.editModel.entry.startDate, this.appService.editModel.entry.endDate, this.taskId, false);
            this.close();
        }
    }

    public delete() {
        if (this.appService.editModel) {
            this.appService.saveEntry(this.appService.editModel.entry.tEntryID, this.appService.editModel.entry.startDate, this.appService.editModel.entry.endDate, this.taskId, true);
            this.close();
        }
    }

    constructor(public appService: AppService) {
    }
}
