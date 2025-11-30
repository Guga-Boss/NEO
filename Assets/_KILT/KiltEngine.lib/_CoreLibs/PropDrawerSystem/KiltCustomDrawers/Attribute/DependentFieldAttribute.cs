using UnityEngine;
using System.Collections;

public enum CheckerFalseDrawOptionEnum {NormalProperty, ReadOnlyProperty, DontDrawProperty}
public enum DependenceOptionEnum {NormalDependence, InverseDependence}

public class DependentFieldAttribute : SpecificFieldAttribute {

	#region Properties

	[SerializeField]
	string m_dependentBoolFieldName = "";
	public string DependentBoolFieldName
	{
		get
		{
			return m_dependentBoolFieldName;
		}
		set
		{
			if(m_dependentBoolFieldName == value)
				return;
			m_dependentBoolFieldName = value;
		}
	}
	
	[SerializeField]
	CheckerFalseDrawOptionEnum m_checkerFalseDrawOption = CheckerFalseDrawOptionEnum.ReadOnlyProperty;
	public CheckerFalseDrawOptionEnum CheckerFalseDrawOption
	{
		get
		{
			return m_checkerFalseDrawOption;
		}
		set
		{
			if(m_checkerFalseDrawOption == value)
				return;
			m_checkerFalseDrawOption = value;
		}
	}

	[SerializeField]
	DependenceOptionEnum m_dependenceOption= DependenceOptionEnum.NormalDependence;
	public DependenceOptionEnum DependenceOption
	{
		get
		{
			return m_dependenceOption;
		}
		set
		{
			if(m_dependenceOption == value)
				return;
			m_dependenceOption = value;
		}
	}
	
	#endregion

	#region Constructor
	
	public DependentFieldAttribute(string p_dependentBoolFieldName) : base(typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, CheckerFalseDrawOptionEnum p_checkerFalseDrawOption) : base(typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_checkerFalseDrawOption = p_checkerFalseDrawOption;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, bool p_readOnly) : base(p_readOnly, typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, CheckerFalseDrawOptionEnum p_checkerFalseDrawOption, bool p_readOnly) : base(p_readOnly, typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_checkerFalseDrawOption = p_checkerFalseDrawOption;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, DependenceOptionEnum p_dependenceOption) : base(typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_dependenceOption = p_dependenceOption;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, DependenceOptionEnum p_dependenceOption, CheckerFalseDrawOptionEnum p_checkerFalseDrawOption) : base(typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_checkerFalseDrawOption = p_checkerFalseDrawOption;
		m_dependenceOption = p_dependenceOption;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, DependenceOptionEnum p_dependenceOption, bool p_readOnly) : base(p_readOnly, typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_dependenceOption = p_dependenceOption;
	}

	public DependentFieldAttribute(string p_dependentBoolFieldName, DependenceOptionEnum p_dependenceOption, CheckerFalseDrawOptionEnum p_checkerFalseDrawOption, bool p_readOnly) : base(p_readOnly, typeof(object))
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_checkerFalseDrawOption = p_checkerFalseDrawOption;
		m_dependenceOption = p_dependenceOption;
	}

	public DependentFieldAttribute(bool p_readOnly, System.Type p_acceptedType, string p_dependentBoolFieldName = "", DependenceOptionEnum p_dependenceOption = DependenceOptionEnum.NormalDependence, CheckerFalseDrawOptionEnum p_checkerFalseDrawOption = CheckerFalseDrawOptionEnum.ReadOnlyProperty) : base(p_readOnly, p_acceptedType)
	{
		m_dependentBoolFieldName = p_dependentBoolFieldName;
		m_checkerFalseDrawOption = p_checkerFalseDrawOption;
		m_dependenceOption = p_dependenceOption;
	}

	#endregion
}
