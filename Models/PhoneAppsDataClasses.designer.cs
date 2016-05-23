﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WMS.Models
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="PhoneApps")]
	public partial class PhoneAppsDataClassesDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region 可扩展性方法定义
    partial void OnCreated();
    partial void InsertApkInfo(ApkInfo instance);
    partial void UpdateApkInfo(ApkInfo instance);
    partial void DeleteApkInfo(ApkInfo instance);
    partial void InsertApkPermission(ApkPermission instance);
    partial void UpdateApkPermission(ApkPermission instance);
    partial void DeleteApkPermission(ApkPermission instance);
    partial void InsertApkDebugInfo(ApkDebugInfo instance);
    partial void UpdateApkDebugInfo(ApkDebugInfo instance);
    partial void DeleteApkDebugInfo(ApkDebugInfo instance);
    partial void InsertApkPermission1(ApkPermission1 instance);
    partial void UpdateApkPermission1(ApkPermission1 instance);
    partial void DeleteApkPermission1(ApkPermission1 instance);
    #endregion
		
		public PhoneAppsDataClassesDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["PhoneAppsConnectionString"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public PhoneAppsDataClassesDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public PhoneAppsDataClassesDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public PhoneAppsDataClassesDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public PhoneAppsDataClassesDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<ApkInfo> ApkInfo
		{
			get
			{
				return this.GetTable<ApkInfo>();
			}
		}
		
		public System.Data.Linq.Table<ApkPermission> ApkPermission
		{
			get
			{
				return this.GetTable<ApkPermission>();
			}
		}
		
		public System.Data.Linq.Table<ApkDebugInfo> ApkDebugInfo
		{
			get
			{
				return this.GetTable<ApkDebugInfo>();
			}
		}
		
		public System.Data.Linq.Table<ApkPermission1> ApkPermission1
		{
			get
			{
				return this.GetTable<ApkPermission1>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ApkInfo")]
	public partial class ApkInfo : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _appname;
		
		private string _versionname;
		
		private string _versioncode;
		
		private string _package;
		
		private System.Nullable<int> _minSdkVersion;
		
		private System.Nullable<int> _targetSdkVersion;
		
		private string _sdkUpdateTime;
		
		private string _updateTime;
		
		private System.Data.Linq.Binary _apk;
		
		private EntitySet<ApkPermission> _ApkPermission;
		
		private EntitySet<ApkDebugInfo> _ApkDebugInfo;
		
		private EntitySet<ApkPermission1> _ApkPermission1;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnappnameChanging(string value);
    partial void OnappnameChanged();
    partial void OnversionnameChanging(string value);
    partial void OnversionnameChanged();
    partial void OnversioncodeChanging(string value);
    partial void OnversioncodeChanged();
    partial void OnpackageChanging(string value);
    partial void OnpackageChanged();
    partial void OnminSdkVersionChanging(System.Nullable<int> value);
    partial void OnminSdkVersionChanged();
    partial void OntargetSdkVersionChanging(System.Nullable<int> value);
    partial void OntargetSdkVersionChanged();
    partial void OnsdkUpdateTimeChanging(string value);
    partial void OnsdkUpdateTimeChanged();
    partial void OnupdateTimeChanging(string value);
    partial void OnupdateTimeChanged();
    partial void OnapkChanging(System.Data.Linq.Binary value);
    partial void OnapkChanged();
    #endregion
		
		public ApkInfo()
		{
			this._ApkPermission = new EntitySet<ApkPermission>(new Action<ApkPermission>(this.attach_ApkPermission), new Action<ApkPermission>(this.detach_ApkPermission));
			this._ApkDebugInfo = new EntitySet<ApkDebugInfo>(new Action<ApkDebugInfo>(this.attach_ApkDebugInfo), new Action<ApkDebugInfo>(this.detach_ApkDebugInfo));
			this._ApkPermission1 = new EntitySet<ApkPermission1>(new Action<ApkPermission1>(this.attach_ApkPermission1), new Action<ApkPermission1>(this.detach_ApkPermission1));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_appname", DbType="VarChar(50) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string appname
		{
			get
			{
				return this._appname;
			}
			set
			{
				if ((this._appname != value))
				{
					this.OnappnameChanging(value);
					this.SendPropertyChanging();
					this._appname = value;
					this.SendPropertyChanged("appname");
					this.OnappnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_versionname", DbType="VarChar(50) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string versionname
		{
			get
			{
				return this._versionname;
			}
			set
			{
				if ((this._versionname != value))
				{
					this.OnversionnameChanging(value);
					this.SendPropertyChanging();
					this._versionname = value;
					this.SendPropertyChanged("versionname");
					this.OnversionnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_versioncode", DbType="VarChar(50)")]
		public string versioncode
		{
			get
			{
				return this._versioncode;
			}
			set
			{
				if ((this._versioncode != value))
				{
					this.OnversioncodeChanging(value);
					this.SendPropertyChanging();
					this._versioncode = value;
					this.SendPropertyChanged("versioncode");
					this.OnversioncodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_package", DbType="VarChar(100)")]
		public string package
		{
			get
			{
				return this._package;
			}
			set
			{
				if ((this._package != value))
				{
					this.OnpackageChanging(value);
					this.SendPropertyChanging();
					this._package = value;
					this.SendPropertyChanged("package");
					this.OnpackageChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_minSdkVersion", DbType="Int")]
		public System.Nullable<int> minSdkVersion
		{
			get
			{
				return this._minSdkVersion;
			}
			set
			{
				if ((this._minSdkVersion != value))
				{
					this.OnminSdkVersionChanging(value);
					this.SendPropertyChanging();
					this._minSdkVersion = value;
					this.SendPropertyChanged("minSdkVersion");
					this.OnminSdkVersionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_targetSdkVersion", DbType="Int")]
		public System.Nullable<int> targetSdkVersion
		{
			get
			{
				return this._targetSdkVersion;
			}
			set
			{
				if ((this._targetSdkVersion != value))
				{
					this.OntargetSdkVersionChanging(value);
					this.SendPropertyChanging();
					this._targetSdkVersion = value;
					this.SendPropertyChanged("targetSdkVersion");
					this.OntargetSdkVersionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_sdkUpdateTime", DbType="VarChar(30)")]
		public string sdkUpdateTime
		{
			get
			{
				return this._sdkUpdateTime;
			}
			set
			{
				if ((this._sdkUpdateTime != value))
				{
					this.OnsdkUpdateTimeChanging(value);
					this.SendPropertyChanging();
					this._sdkUpdateTime = value;
					this.SendPropertyChanged("sdkUpdateTime");
					this.OnsdkUpdateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_updateTime", DbType="VarChar(30) NOT NULL", CanBeNull=false)]
		public string updateTime
		{
			get
			{
				return this._updateTime;
			}
			set
			{
				if ((this._updateTime != value))
				{
					this.OnupdateTimeChanging(value);
					this.SendPropertyChanging();
					this._updateTime = value;
					this.SendPropertyChanged("updateTime");
					this.OnupdateTimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_apk", DbType="Image", UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary apk
		{
			get
			{
				return this._apk;
			}
			set
			{
				if ((this._apk != value))
				{
					this.OnapkChanging(value);
					this.SendPropertyChanging();
					this._apk = value;
					this.SendPropertyChanged("apk");
					this.OnapkChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ApkInfo_ApkPermission", Storage="_ApkPermission", ThisKey="appname,versionname", OtherKey="appname,versionname")]
		public EntitySet<ApkPermission> ApkPermission
		{
			get
			{
				return this._ApkPermission;
			}
			set
			{
				this._ApkPermission.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ApkInfo_ApkDebugInfo", Storage="_ApkDebugInfo", ThisKey="appname,versionname", OtherKey="appname,versionname")]
		public EntitySet<ApkDebugInfo> ApkDebugInfo
		{
			get
			{
				return this._ApkDebugInfo;
			}
			set
			{
				this._ApkDebugInfo.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ApkInfo_ApkPermission1", Storage="_ApkPermission1", ThisKey="appname,versionname", OtherKey="appname,versionname")]
		public EntitySet<ApkPermission1> ApkPermission1
		{
			get
			{
				return this._ApkPermission1;
			}
			set
			{
				this._ApkPermission1.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_ApkPermission(ApkPermission entity)
		{
			this.SendPropertyChanging();
			entity.ApkInfo = this;
		}
		
		private void detach_ApkPermission(ApkPermission entity)
		{
			this.SendPropertyChanging();
			entity.ApkInfo = null;
		}
		
		private void attach_ApkDebugInfo(ApkDebugInfo entity)
		{
			this.SendPropertyChanging();
			entity.ApkInfo = this;
		}
		
		private void detach_ApkDebugInfo(ApkDebugInfo entity)
		{
			this.SendPropertyChanging();
			entity.ApkInfo = null;
		}
		
		private void attach_ApkPermission1(ApkPermission1 entity)
		{
			this.SendPropertyChanging();
			entity.ApkInfo = this;
		}
		
		private void detach_ApkPermission1(ApkPermission1 entity)
		{
			this.SendPropertyChanging();
			entity.ApkInfo = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ApkPermission")]
	public partial class ApkPermission : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _actid;
		
		private string _appname;
		
		private string _versionname;
		
		private string _permission;
		
		private EntityRef<ApkInfo> _ApkInfo;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnactidChanging(int value);
    partial void OnactidChanged();
    partial void OnappnameChanging(string value);
    partial void OnappnameChanged();
    partial void OnversionnameChanging(string value);
    partial void OnversionnameChanged();
    partial void OnpermissionChanging(string value);
    partial void OnpermissionChanged();
    #endregion
		
		public ApkPermission()
		{
			this._ApkInfo = default(EntityRef<ApkInfo>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_actid", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int actid
		{
			get
			{
				return this._actid;
			}
			set
			{
				if ((this._actid != value))
				{
					this.OnactidChanging(value);
					this.SendPropertyChanging();
					this._actid = value;
					this.SendPropertyChanged("actid");
					this.OnactidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_appname", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string appname
		{
			get
			{
				return this._appname;
			}
			set
			{
				if ((this._appname != value))
				{
					if (this._ApkInfo.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnappnameChanging(value);
					this.SendPropertyChanging();
					this._appname = value;
					this.SendPropertyChanged("appname");
					this.OnappnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_versionname", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string versionname
		{
			get
			{
				return this._versionname;
			}
			set
			{
				if ((this._versionname != value))
				{
					if (this._ApkInfo.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnversionnameChanging(value);
					this.SendPropertyChanging();
					this._versionname = value;
					this.SendPropertyChanged("versionname");
					this.OnversionnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_permission", DbType="VarChar(200)")]
		public string permission
		{
			get
			{
				return this._permission;
			}
			set
			{
				if ((this._permission != value))
				{
					this.OnpermissionChanging(value);
					this.SendPropertyChanging();
					this._permission = value;
					this.SendPropertyChanged("permission");
					this.OnpermissionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ApkInfo_ApkPermission", Storage="_ApkInfo", ThisKey="appname,versionname", OtherKey="appname,versionname", IsForeignKey=true)]
		public ApkInfo ApkInfo
		{
			get
			{
				return this._ApkInfo.Entity;
			}
			set
			{
				ApkInfo previousValue = this._ApkInfo.Entity;
				if (((previousValue != value) 
							|| (this._ApkInfo.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ApkInfo.Entity = null;
						previousValue.ApkPermission.Remove(this);
					}
					this._ApkInfo.Entity = value;
					if ((value != null))
					{
						value.ApkPermission.Add(this);
						this._appname = value.appname;
						this._versionname = value.versionname;
					}
					else
					{
						this._appname = default(string);
						this._versionname = default(string);
					}
					this.SendPropertyChanged("ApkInfo");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ApkDebugInfo")]
	public partial class ApkDebugInfo : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _actid;
		
		private string _appname;
		
		private string _versionname;
		
		private string _DebugItem;
		
		private EntityRef<ApkInfo> _ApkInfo;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnactidChanging(int value);
    partial void OnactidChanged();
    partial void OnappnameChanging(string value);
    partial void OnappnameChanged();
    partial void OnversionnameChanging(string value);
    partial void OnversionnameChanged();
    partial void OnDebugItemChanging(string value);
    partial void OnDebugItemChanged();
    #endregion
		
		public ApkDebugInfo()
		{
			this._ApkInfo = default(EntityRef<ApkInfo>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_actid", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int actid
		{
			get
			{
				return this._actid;
			}
			set
			{
				if ((this._actid != value))
				{
					this.OnactidChanging(value);
					this.SendPropertyChanging();
					this._actid = value;
					this.SendPropertyChanged("actid");
					this.OnactidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_appname", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string appname
		{
			get
			{
				return this._appname;
			}
			set
			{
				if ((this._appname != value))
				{
					if (this._ApkInfo.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnappnameChanging(value);
					this.SendPropertyChanging();
					this._appname = value;
					this.SendPropertyChanged("appname");
					this.OnappnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_versionname", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string versionname
		{
			get
			{
				return this._versionname;
			}
			set
			{
				if ((this._versionname != value))
				{
					if (this._ApkInfo.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnversionnameChanging(value);
					this.SendPropertyChanging();
					this._versionname = value;
					this.SendPropertyChanged("versionname");
					this.OnversionnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DebugItem", DbType="Text", UpdateCheck=UpdateCheck.Never)]
		public string DebugItem
		{
			get
			{
				return this._DebugItem;
			}
			set
			{
				if ((this._DebugItem != value))
				{
					this.OnDebugItemChanging(value);
					this.SendPropertyChanging();
					this._DebugItem = value;
					this.SendPropertyChanged("DebugItem");
					this.OnDebugItemChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ApkInfo_ApkDebugInfo", Storage="_ApkInfo", ThisKey="appname,versionname", OtherKey="appname,versionname", IsForeignKey=true)]
		public ApkInfo ApkInfo
		{
			get
			{
				return this._ApkInfo.Entity;
			}
			set
			{
				ApkInfo previousValue = this._ApkInfo.Entity;
				if (((previousValue != value) 
							|| (this._ApkInfo.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ApkInfo.Entity = null;
						previousValue.ApkDebugInfo.Remove(this);
					}
					this._ApkInfo.Entity = value;
					if ((value != null))
					{
						value.ApkDebugInfo.Add(this);
						this._appname = value.appname;
						this._versionname = value.versionname;
					}
					else
					{
						this._appname = default(string);
						this._versionname = default(string);
					}
					this.SendPropertyChanged("ApkInfo");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ApkPermission")]
	public partial class ApkPermission1 : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _actid;
		
		private string _appname;
		
		private string _versionname;
		
		private string _permission;
		
		private EntityRef<ApkInfo> _ApkInfo;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnactidChanging(int value);
    partial void OnactidChanged();
    partial void OnappnameChanging(string value);
    partial void OnappnameChanged();
    partial void OnversionnameChanging(string value);
    partial void OnversionnameChanged();
    partial void OnpermissionChanging(string value);
    partial void OnpermissionChanged();
    #endregion
		
		public ApkPermission1()
		{
			this._ApkInfo = default(EntityRef<ApkInfo>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_actid", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int actid
		{
			get
			{
				return this._actid;
			}
			set
			{
				if ((this._actid != value))
				{
					this.OnactidChanging(value);
					this.SendPropertyChanging();
					this._actid = value;
					this.SendPropertyChanged("actid");
					this.OnactidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_appname", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string appname
		{
			get
			{
				return this._appname;
			}
			set
			{
				if ((this._appname != value))
				{
					if (this._ApkInfo.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnappnameChanging(value);
					this.SendPropertyChanging();
					this._appname = value;
					this.SendPropertyChanged("appname");
					this.OnappnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_versionname", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string versionname
		{
			get
			{
				return this._versionname;
			}
			set
			{
				if ((this._versionname != value))
				{
					if (this._ApkInfo.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnversionnameChanging(value);
					this.SendPropertyChanging();
					this._versionname = value;
					this.SendPropertyChanged("versionname");
					this.OnversionnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_permission", DbType="VarChar(200)")]
		public string permission
		{
			get
			{
				return this._permission;
			}
			set
			{
				if ((this._permission != value))
				{
					this.OnpermissionChanging(value);
					this.SendPropertyChanging();
					this._permission = value;
					this.SendPropertyChanged("permission");
					this.OnpermissionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ApkInfo_ApkPermission1", Storage="_ApkInfo", ThisKey="appname,versionname", OtherKey="appname,versionname", IsForeignKey=true)]
		public ApkInfo ApkInfo
		{
			get
			{
				return this._ApkInfo.Entity;
			}
			set
			{
				ApkInfo previousValue = this._ApkInfo.Entity;
				if (((previousValue != value) 
							|| (this._ApkInfo.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ApkInfo.Entity = null;
						previousValue.ApkPermission1.Remove(this);
					}
					this._ApkInfo.Entity = value;
					if ((value != null))
					{
						value.ApkPermission1.Add(this);
						this._appname = value.appname;
						this._versionname = value.versionname;
					}
					else
					{
						this._appname = default(string);
						this._versionname = default(string);
					}
					this.SendPropertyChanged("ApkInfo");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591