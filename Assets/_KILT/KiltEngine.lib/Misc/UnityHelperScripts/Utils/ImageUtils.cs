using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ImageUtils 
{
	public static bool ClampSize(this Texture2D p_texture, float p_minSize = 2, float p_maxSize = 512) 
	{
		bool v_return = false;
		p_maxSize = Mathf.Max(2, p_maxSize);
		p_minSize = Mathf.Max(2, p_minSize);
		if (p_texture != null) 
		{
			bool v_needApplyMin = p_texture.width < p_minSize ||  p_texture.height < p_minSize;
			bool v_needApplyMax = p_texture.width > p_maxSize ||  p_texture.height > p_maxSize;
			if(v_needApplyMin || v_needApplyMax)
			{
				try
				{
					float v_sizeToApply = v_needApplyMin? p_minSize : p_maxSize;
					float v_widthRatio = (float)v_sizeToApply / p_texture.width;
					float v_heightRatio = (float)v_sizeToApply / p_texture.height;
					if (v_widthRatio < v_heightRatio) 
						v_return = p_texture.Resize((int)(p_texture.width * v_widthRatio), (int)(p_texture.height * v_widthRatio));
					else 
						v_return = p_texture.Resize((int)(p_texture.width * v_heightRatio), (int)(p_texture.height * v_heightRatio));
				}
				catch{}
			}
		}
		return v_return;
	}
}
