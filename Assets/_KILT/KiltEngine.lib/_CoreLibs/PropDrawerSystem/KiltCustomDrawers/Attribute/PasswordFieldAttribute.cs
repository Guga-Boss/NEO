using UnityEngine;
using System.Collections;

public class PasswordFieldAttribute : SpecificFieldAttribute {
	
	#region Constructor
	
	public PasswordFieldAttribute() : base(typeof(string))
	{
	}
	
	public PasswordFieldAttribute(bool p_readOnly) : base(p_readOnly, typeof(string))
	{
	}
	
	#endregion
}
