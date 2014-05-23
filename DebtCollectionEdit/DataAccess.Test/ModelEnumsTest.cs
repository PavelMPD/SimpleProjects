using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Diagnostics;
using DebtCollection.Model.Enums;
using NUnit.Framework;
using System.Linq;
using DebtCollection.Model;
using DebtCollection.Common;

namespace DataAccess.Test
{
    [TestFixture]
    public class ModelEnumsTest
    {
        [Test]
        public void ConvertStageEnum()
        {
            foreach (Stage stage in Enum.GetValues(typeof (Stage)))
            {
                Trace.WriteLine(String.Format("SELECT {0} [Id], '{1}' [Name], '{2}' [Description] UNION", (Int32) stage,
                                              stage, EnumHelper.GetDisplayValue(stage)));
            }
        }

        [Test]
        public void ConvertSubscriberStatusEnum()
        {
            foreach (SubscriberStatus status in Enum.GetValues(typeof (SubscriberStatus)))
            {
                Trace.WriteLine(String.Format("SELECT {0} [Id], '{1}' [Name], {2} [Order], '{3}' [Description] UNION",
                                              (Int32) status, status, EnumHelper.GetOrderValue(status),
                                              EnumHelper.GetDisplayValue(status)));
            }
        }

        [Test]
        public void ConvertFileEntityTypeEnum()
        {
            foreach (FileEntityType entity in Enum.GetValues(typeof (FileEntityType)))
            {
                Trace.WriteLine(String.Format("SELECT {0} [Id], '{1}' [Name] UNION", (Int32) entity, entity));
            }
        }

        [Test]
        public void ConvertOperatingAccountEnum()
        {
            foreach (var entity in EnumToList.Conver<OperatingAccount>())
            {
                Trace.WriteLine(entity.ToString());
            }
        }
    }

    public class EnumToList
    {
        public static IList<EnumEntity> Conver<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Select(e => new EnumEntity() { Id = Convert.ToInt32(e), Name = e.ToString(), Description = EnumHelper.GetDisplayValue(e) }).ToList();
        }
    }

    public class EnumEntity
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }

        public override string ToString()
        {
            return String.Format("SELECT {0} [Id], '{1}' [Name], '{2}' [Description] UNION",
                                              Id,
                                              Name,
                                              Description);
        }
    }
}
