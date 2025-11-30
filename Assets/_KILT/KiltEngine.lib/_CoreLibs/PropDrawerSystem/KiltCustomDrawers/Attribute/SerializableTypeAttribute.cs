using UnityEngine;
using System.Collections;

public class SerializableTypeAttribute : DependentFieldAttribute {
	
	#region Properties
	
	[SerializeField]
	System.Type m_filterType = null;
	public System.Type FilterType
	{
		get
		{
			return m_filterType;
		}
		set
		{
			if(m_filterType == value)
				return;
			m_filterType = value;
		}
	}

	[SerializeField]
	bool m_acceptGenericDefinitions = false;
	public bool AcceptGenericDefinitions
	{
		get
		{
			return m_acceptGenericDefinitions;
		}
		set
		{
			if(m_acceptGenericDefinitions == value)
				return;
			m_acceptGenericDefinitions = value;
		}
	}

	[SerializeField]
	bool m_acceptAbstractDefinitions = false;
	public bool AcceptAbstractDefinitions
	{
		get
		{
			return m_acceptAbstractDefinitions;
		}
		set
		{
			if(m_acceptAbstractDefinitions == value)
				return;
			m_acceptAbstractDefinitions = value;
		}
	}
	
	#endregion
	
	#region Constructor
	
	public SerializableTypeAttribute() : base(false, typeof(SerializableType))
	{
	}
	
	public SerializableTypeAttribute(bool p_readOnly) : base(p_readOnly, typeof(SerializableType))
	{
	}

	public SerializableTypeAttribute(System.Type p_filterType, bool p_acceptGenericDefinition = false, bool p_acceptAbstractDefinitions = false, bool p_readOnly = false, string p_dependentBoolFieldName = "", DependenceOptionEnum p_dependenceOption = DependenceOptionEnum.NormalDependence, CheckerFalseDrawOptionEnum p_checkerFalseDrawOption = CheckerFalseDrawOptionEnum.ReadOnlyProperty) : base(p_readOnly, typeof(SerializableType), p_dependentBoolFieldName, p_dependenceOption, p_checkerFalseDrawOption)
	{
		m_filterType = p_filterType;
		m_acceptGenericDefinitions = p_acceptGenericDefinition;
		m_acceptAbstractDefinitions = p_acceptAbstractDefinitions;
	}
	
	#endregion
}
