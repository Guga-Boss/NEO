using UnityEngine;
using System.Collections;

public abstract class GenericColorBehavior : MonoBehaviour
{
	public virtual Color Color {get; set;}
	public virtual float ColorIntensity {get; set;}
}
