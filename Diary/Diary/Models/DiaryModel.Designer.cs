﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]
#region EDM Relationship Metadata

[assembly: EdmRelationshipAttribute("DIARYModel", "FK_Task_TaskStatus", "TaskStatus", System.Data.Metadata.Edm.RelationshipMultiplicity.One, typeof(Diary.Models.TaskStatus), "Task", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Diary.Models.Task), true)]
[assembly: EdmRelationshipAttribute("DIARYModel", "FK_Task_WorkDay", "WorkDay", System.Data.Metadata.Edm.RelationshipMultiplicity.One, typeof(Diary.Models.WorkDay), "Task", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Diary.Models.Task), true)]

#endregion

namespace Diary.Models
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class DIARYEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new DIARYEntities object using the connection string found in the 'DIARYEntities' section of the application configuration file.
        /// </summary>
        public DIARYEntities() : base("name=DIARYEntities", "DIARYEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new DIARYEntities object.
        /// </summary>
        public DIARYEntities(string connectionString) : base(connectionString, "DIARYEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new DIARYEntities object.
        /// </summary>
        public DIARYEntities(EntityConnection connection) : base(connection, "DIARYEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Task> Task
        {
            get
            {
                if ((_Task == null))
                {
                    _Task = base.CreateObjectSet<Task>("Task");
                }
                return _Task;
            }
        }
        private ObjectSet<Task> _Task;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<TaskStatus> TaskStatus
        {
            get
            {
                if ((_TaskStatus == null))
                {
                    _TaskStatus = base.CreateObjectSet<TaskStatus>("TaskStatus");
                }
                return _TaskStatus;
            }
        }
        private ObjectSet<TaskStatus> _TaskStatus;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<WorkDay> WorkDay
        {
            get
            {
                if ((_WorkDay == null))
                {
                    _WorkDay = base.CreateObjectSet<WorkDay>("WorkDay");
                }
                return _WorkDay;
            }
        }
        private ObjectSet<WorkDay> _WorkDay;

        #endregion
        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Task EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToTask(Task task)
        {
            base.AddObject("Task", task);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the TaskStatus EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToTaskStatus(TaskStatus taskStatus)
        {
            base.AddObject("TaskStatus", taskStatus);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the WorkDay EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToWorkDay(WorkDay workDay)
        {
            base.AddObject("WorkDay", workDay);
        }

        #endregion
    }
    

    #endregion
    
    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="DIARYModel", Name="Task")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Task : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Task object.
        /// </summary>
        /// <param name="id">Initial value of the Id property.</param>
        /// <param name="workDayId">Initial value of the WorkDayId property.</param>
        /// <param name="taskStatusId">Initial value of the TaskStatusId property.</param>
        public static Task CreateTask(global::System.Int64 id, global::System.Int64 workDayId, global::System.Int32 taskStatusId)
        {
            Task task = new Task();
            task.Id = id;
            task.WorkDayId = workDayId;
            task.TaskStatusId = taskStatusId;
            return task;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    ReportPropertyChanging("Id");
                    _Id = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }
        private global::System.Int64 _Id;
        partial void OnIdChanging(global::System.Int64 value);
        partial void OnIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 WorkDayId
        {
            get
            {
                return _WorkDayId;
            }
            set
            {
                OnWorkDayIdChanging(value);
                ReportPropertyChanging("WorkDayId");
                _WorkDayId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WorkDayId");
                OnWorkDayIdChanged();
            }
        }
        private global::System.Int64 _WorkDayId;
        partial void OnWorkDayIdChanging(global::System.Int64 value);
        partial void OnWorkDayIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                OnCaptionChanging(value);
                ReportPropertyChanging("Caption");
                _Caption = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Caption");
                OnCaptionChanged();
            }
        }
        private global::System.String _Caption;
        partial void OnCaptionChanging(global::System.String value);
        partial void OnCaptionChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Description
        {
            get
            {
                return _Description;
            }
            set
            {
                OnDescriptionChanging(value);
                ReportPropertyChanging("Description");
                _Description = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Description");
                OnDescriptionChanged();
            }
        }
        private global::System.String _Description;
        partial void OnDescriptionChanging(global::System.String value);
        partial void OnDescriptionChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 TaskStatusId
        {
            get
            {
                return _TaskStatusId;
            }
            set
            {
                OnTaskStatusIdChanging(value);
                ReportPropertyChanging("TaskStatusId");
                _TaskStatusId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TaskStatusId");
                OnTaskStatusIdChanged();
            }
        }
        private global::System.Int32 _TaskStatusId;
        partial void OnTaskStatusIdChanging(global::System.Int32 value);
        partial void OnTaskStatusIdChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("DIARYModel", "FK_Task_TaskStatus", "TaskStatus")]
        public TaskStatus TaskStatus
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TaskStatus>("DIARYModel.FK_Task_TaskStatus", "TaskStatus").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TaskStatus>("DIARYModel.FK_Task_TaskStatus", "TaskStatus").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<TaskStatus> TaskStatusReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<TaskStatus>("DIARYModel.FK_Task_TaskStatus", "TaskStatus");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<TaskStatus>("DIARYModel.FK_Task_TaskStatus", "TaskStatus", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("DIARYModel", "FK_Task_WorkDay", "WorkDay")]
        public WorkDay WorkDay
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<WorkDay>("DIARYModel.FK_Task_WorkDay", "WorkDay").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<WorkDay>("DIARYModel.FK_Task_WorkDay", "WorkDay").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<WorkDay> WorkDayReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<WorkDay>("DIARYModel.FK_Task_WorkDay", "WorkDay");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<WorkDay>("DIARYModel.FK_Task_WorkDay", "WorkDay", value);
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="DIARYModel", Name="TaskStatus")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class TaskStatus : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new TaskStatus object.
        /// </summary>
        /// <param name="id">Initial value of the Id property.</param>
        public static TaskStatus CreateTaskStatus(global::System.Int32 id)
        {
            TaskStatus taskStatus = new TaskStatus();
            taskStatus.Id = id;
            return taskStatus;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    ReportPropertyChanging("Id");
                    _Id = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }
        private global::System.Int32 _Id;
        partial void OnIdChanging(global::System.Int32 value);
        partial void OnIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging("Name");
                _Name = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Name");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("DIARYModel", "FK_Task_TaskStatus", "Task")]
        public EntityCollection<Task> Task
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<Task>("DIARYModel.FK_Task_TaskStatus", "Task");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<Task>("DIARYModel.FK_Task_TaskStatus", "Task", value);
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="DIARYModel", Name="WorkDay")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class WorkDay : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new WorkDay object.
        /// </summary>
        /// <param name="id">Initial value of the Id property.</param>
        /// <param name="date">Initial value of the Date property.</param>
        /// <param name="userName">Initial value of the UserName property.</param>
        public static WorkDay CreateWorkDay(global::System.Int64 id, global::System.DateTime date, global::System.String userName)
        {
            WorkDay workDay = new WorkDay();
            workDay.Id = id;
            workDay.Date = date;
            workDay.UserName = userName;
            return workDay;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int64 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    ReportPropertyChanging("Id");
                    _Id = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }
        private global::System.Int64 _Id;
        partial void OnIdChanging(global::System.Int64 value);
        partial void OnIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Date
        {
            get
            {
                return _Date;
            }
            set
            {
                OnDateChanging(value);
                ReportPropertyChanging("Date");
                _Date = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Date");
                OnDateChanged();
            }
        }
        private global::System.DateTime _Date;
        partial void OnDateChanging(global::System.DateTime value);
        partial void OnDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                OnUserNameChanging(value);
                ReportPropertyChanging("UserName");
                _UserName = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("UserName");
                OnUserNameChanged();
            }
        }
        private global::System.String _UserName;
        partial void OnUserNameChanging(global::System.String value);
        partial void OnUserNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Comment
        {
            get
            {
                return _Comment;
            }
            set
            {
                OnCommentChanging(value);
                ReportPropertyChanging("Comment");
                _Comment = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Comment");
                OnCommentChanged();
            }
        }
        private global::System.String _Comment;
        partial void OnCommentChanging(global::System.String value);
        partial void OnCommentChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("DIARYModel", "FK_Task_WorkDay", "Task")]
        public EntityCollection<Task> Task
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<Task>("DIARYModel.FK_Task_WorkDay", "Task");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<Task>("DIARYModel.FK_Task_WorkDay", "Task", value);
                }
            }
        }

        #endregion
    }

    #endregion
    
}
