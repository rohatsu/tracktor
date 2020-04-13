// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

//using Autofac;
using AutoMapper;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using tracktor.model.DAL;

namespace tracktor.service
{
    public static class TracktorStartup
    {
        public static Mapper AppInitialize()
        {
            // autofac
            /*var builder = new ContainerBuilder();
            builder.RegisterType<TracktorService>();
            builder.RegisterType<TracktorContext>().As<ITracktorContext>();
            builder.Register<ILog>((c, p) => LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)).SingleInstance();
            var container = builder.Build();*/

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<TProject, TProjectDto>().AfterMap((src, dst) =>
                    dst.InProgress = false);

                cfg.CreateMap<TTask, TTaskDto>().AfterMap((src, dst) =>
                {
                    dst.Contrib = new TContribDto { Today = 0, ThisWeek = 0, ThisMonth = 0 };
                    dst.InProgress = false;
                });

                cfg.CreateMap<TEntry, TEntryDto>().AfterMap((src, dst) =>
                {
                    dst.Contrib = 0;
                    dst.InProgress = false;
                    dst.IsDeleted = false;
                });

                cfg.CreateMap<TracktorReport, TReportModelDto>();
            });

            return new Mapper(config);

//            AutofacHostFactory.Container = container;

        }

        /*public static void ConfigureAutofac(ContainerBuilder builder)
        {
            builder.RegisterType<TracktorContext>().As<ITracktorContext>();
            builder.Register<ILog>((c, p) => LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)).SingleInstance();
        }*/
    }
}