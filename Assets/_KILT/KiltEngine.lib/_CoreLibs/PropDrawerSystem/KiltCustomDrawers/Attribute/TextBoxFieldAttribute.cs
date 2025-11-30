using UnityEngine;
using System.Collections;

public class TextBoxFieldAttribute : SpecificFieldAttribute {

	#region Properties
	
	[SerializeField]
	float m_guiHeight = 70;
	public float GuiHeight
	{
		get
		{
			return m_guiHeight;
		}
		set
		{
			if(m_guiHeight == value)
				return;
			m_guiHeight = value;
		}
	}
	
	#endregion

	#region Constructor

	public TextBoxFieldAttribute() : base(typeof(string))
	{
	}
	
	public TextBoxFieldAttribute(float p_guiHeight) : base(typeof(string))
	{
		m_guiHeight = p_guiHeight;
	}

	public TextBoxFieldAttribute(bool p_readOnly) : base(p_readOnly, typeof(string))
	{
	}
	
	public TextBoxFieldAttribute(float p_guiHeight, bool p_readOnly) : base(p_readOnly, typeof(string))
	{
		m_guiHeight = p_guiHeight;
	}
	
	#endregion
}
