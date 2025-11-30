using System.Collections;

public class Delegates {

	public delegate void EventHandler<T, K, H> (T e, K e2, H e3);
	public delegate void EventHandler<T, K> (T e, K e2);
	public delegate void EventHandler<T> (T e);
	public delegate void EventHandler ();

	public delegate void FunctionPointer<T, K, H> (T e, K e2, H e3);
	public delegate void FunctionPointer<T, K>(T e, K e2);
	public delegate void FunctionPointer<T>(T e);

	//public delegate void FunctionPointerThree(object e, object e2, object e3);
	//public delegate void FunctionPointerTwo(object e, object e2);
	//public delegate void FunctionPointerOne(object e);
	public delegate void FunctionPointer();
}

