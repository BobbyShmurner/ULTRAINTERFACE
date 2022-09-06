using System;

[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
public class DontDestroyGameObjectOnUnloadAttribute : Attribute {
	
}