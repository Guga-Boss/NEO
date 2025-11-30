using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

//XML and BXML need ".net 2.0 subset" to run in IOS Devices. 
//You cannot use SendMailButton with XMLSerialization in IOS because SendMail need ".net 2.0"
//Use AutoDetect to try All Serialization Methods
public enum SerializationTypeEnum {AutoDetect, JSON, BSON, XML, BXML};

public class SerializerHelper 
{
	#region Strong-Typed Serialization/Deserialization

	#region Clone Methods

	//Only Works in System.Serializable Objects
	public static T CloneBySerialization<T>(T p_object)
	{
		T v_clonedObject = default(T);
		
		try
		{
			//This string is the JSON representation of the object
			string v_serialized = JsonConvert.SerializeObject(p_object);
			//Now we can deserialize this string back into an object
			v_clonedObject = JsonConvert.DeserializeObject<T>(v_serialized);
		}
		catch{}
		
		return v_clonedObject;
	}
	
	#endregion
	
	#region Type Serialization

	#region To String

	public static string SerializeJSONToString<T>(T p_object)
	{
		string v_textObject = "";
		try
		{
			if(p_object != null)
			{
				JsonSerializerSettings v_settings = new JsonSerializerSettings();
				v_settings.TypeNameHandling = TypeNameHandling.All;
				v_textObject = JsonConvert.SerializeObject(p_object, Newtonsoft.Json.Formatting.Indented, v_settings);
			}
		}
		catch {}
		
		return v_textObject;
	}
	
	public static string SerializeBSONToString<T>(T p_object)
	{
		string v_encodedObject = "";
		try
		{
			if(p_object != null)
			{
				using(MemoryStream v_memoryStream  = new MemoryStream())
				{
					using (BsonWriter v_bsonWriter = new BsonWriter(v_memoryStream))
					{
						JsonSerializerSettings v_settings = new JsonSerializerSettings();
						v_settings.TypeNameHandling = TypeNameHandling.All;
						JsonSerializer v_jsonSerializer = JsonSerializer.Create(v_settings); //new JsonSerializer();
						v_jsonSerializer.Serialize(v_bsonWriter, p_object);
					}
					
					//Read the stream to a byte array.  We could 
					//just as easily output it to a file
					byte[] v_serializedData = v_memoryStream.ToArray();
					v_encodedObject = Convert.ToBase64String(v_serializedData);
				}
			}
		}
		catch {}
		
		return v_encodedObject;
	}

	public static string SerializeXMLToString<T>(T p_object)
	{
		string v_textObject = "";
		try
		{
			if(p_object != null)
			{
				using(MemoryStream v_memoryStream = new MemoryStream())
				{
					// Create a new XmlSerializer instance with the type of the test class
					XmlSerializer v_serializer = new XmlSerializer(typeof(T));
					
					v_serializer.Serialize(v_memoryStream, p_object);
					// Cleanup
					byte[] v_serializedData = v_memoryStream.ToArray();
					v_textObject = Convert.ToBase64String(v_serializedData);
				}
			}
		}
		catch {}
		
		return v_textObject;
	}
	
	public static string SerializeBXMLToString<T>(T p_object)
	{
		string v_textObject = SerializeXMLToString<T>(p_object);
		string v_encodedObject = EncodeTo64(v_textObject);
		return v_encodedObject;
	}

	#endregion

	#endregion
	
	#region Type Deserialization

	#region To String

	public static T DeserializeJSONFromString<T>(string p_textObject, JsonSerializerSettings p_settings)
	{   
		T v_object = default(T);
		try
		{
			v_object = JsonConvert.DeserializeObject<T>(p_textObject, p_settings);
		}
		catch {}
		
		return v_object;
	}

	public static T DeserializeJSONFromString<T>(string p_textObject)
	{   
		T v_object = default(T);
		try
		{
			JsonSerializerSettings v_settings = new JsonSerializerSettings();
			v_settings.TypeNameHandling = TypeNameHandling.All;
			v_object = JsonConvert.DeserializeObject<T>(p_textObject, v_settings);
		}
		catch {}
		
		return v_object;
	}

	public static T DeserializeBSONFromString<T>(string p_encodedObject)
	{   
		T v_object = default(T);
		try
		{
			//Now that we have a byte array of our serialized data, let's Deserialize it.
			byte[] v_serializedData = System.Convert.FromBase64String(p_encodedObject);
			using (MemoryStream v_stream = new MemoryStream(v_serializedData))
			{
				using (BsonReader v_reader = new BsonReader(v_stream))
				{
					//If you're deserializing a collection, the following option 
					//must be set to instruct the reader that the root object 
					//is actually an array / collection type.
					//Type v_type = typeof(T);
					//if(KiltUtils.IsSameOrSubClassOrImplementInterface(v_type ,typeof(ICollection)) ||
					//   KiltUtils.IsSameOrSubClassOrImplementInterface(v_type ,typeof(IEnumerable)) ||
					//   v_type.IsArray)
					//	v_reader.ReadRootValueAsArray = true;
					
					JsonSerializerSettings v_settings = new JsonSerializerSettings();
					v_settings.TypeNameHandling = TypeNameHandling.All;
					JsonSerializer v_serializer = JsonSerializer.Create(v_settings);
					v_object = v_serializer.Deserialize<T>(v_reader);
				}
			}
		}
		catch {}
		
		return v_object;
	}

	public static T DeserializeXMLFromString<T>(string p_textObject)
	{   
		T v_object = default(T);
		try
		{
			//Set up serializer of the correct type & place text into a string reader
			XmlSerializer v_serializer = new XmlSerializer(typeof(T));
			using(StringReader v_strReader = new StringReader(p_textObject))
			{
				//Use the string reader to create an object that can be deserialized
				using(XmlTextReader v_xmlFromText = new XmlTextReader(v_strReader))
				{
					//Deserialize file to return, closing my Readers before returning 
					v_object = (T)v_serializer.Deserialize(v_xmlFromText);
					v_xmlFromText.Close();
				}
			}
			return v_object;
		}
		catch {}
		
		return v_object;
	}

	public static T DeserializeBXMLFromString<T>(string p_encodedObject)
	{   
		string v_textObject = DecodeFrom64(p_encodedObject);
		T v_object = DeserializeXMLFromString<T>(v_textObject);
		return v_object;
	}

	#endregion
	
	#endregion
	
	#region Serialize/Deserialize

	#region From/To String

	public static string SerializeToString<T>(T p_object, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		string v_textObject = "";
		try
		{
			if(string.IsNullOrEmpty(v_textObject) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.BSON))
				v_textObject = SerializeBSONToString<T>(p_object);
			if(string.IsNullOrEmpty(v_textObject) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.JSON))
				v_textObject = SerializeJSONToString<T>(p_object);
			if(string.IsNullOrEmpty(v_textObject) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.BXML))
				v_textObject = SerializeBXMLToString<T>(p_object);
			if(string.IsNullOrEmpty(v_textObject) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.XML))
				v_textObject = SerializeXMLToString<T>(p_object);
		}
		catch {}
		
		return v_textObject;
	}

	public static T DeserializeFromString<T>(string p_textObject, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		T v_desserializedObject = default(T);
		try
		{
			if(EqualityComparer<T>.Default.Equals(v_desserializedObject, default(T)) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.BSON))
				v_desserializedObject = DeserializeBSONFromString<T>(p_textObject);
			if(EqualityComparer<T>.Default.Equals(v_desserializedObject, default(T)) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.JSON))
				v_desserializedObject = DeserializeJSONFromString<T>(p_textObject);
			if(EqualityComparer<T>.Default.Equals(v_desserializedObject, default(T)) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.BXML))
				v_desserializedObject = DeserializeBXMLFromString<T>(p_textObject);
			if(EqualityComparer<T>.Default.Equals(v_desserializedObject, default(T)) && (p_serializationType == SerializationTypeEnum.AutoDetect || p_serializationType == SerializationTypeEnum.XML))
				v_desserializedObject = DeserializeXMLFromString<T>(p_textObject);
		}
		catch {}
		
		return v_desserializedObject;
	}

	#endregion

	#region From/To File

	//Always Serialize Outside Resources in Phone Devices
	public static bool Serialize<T>(T p_object, string p_folder, string p_file, bool p_folderIsRelativePath = true, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		string v_resultPath = KiltUtils.GetCompleteDataPath(p_folder, p_file, p_folderIsRelativePath);
		return Serialize<T>(p_object, v_resultPath, p_serializationType);
	}

	//Always Serialize Outside Resources in Phone Devices
	public static bool Serialize<T>(T p_object, string p_fullPathWithFileName, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		bool v_sucess = false;
		try
		{
			string v_resultPath = p_fullPathWithFileName;

			string v_stringObject = SerializeToString(p_object, p_serializationType);
			WriteAllText(v_stringObject, v_resultPath);
		}
		catch {}
		
		return v_sucess;
	}

	public static T Deserialize<T>(string p_folder, string p_file, bool p_folderIsRelativePath = true, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		string v_resultPath = KiltUtils.GetCompleteDataPath(p_folder, p_file, p_folderIsRelativePath);
		T v_desserializedObject = Deserialize<T>(v_resultPath, p_serializationType);
		
		return v_desserializedObject;
	}

	public static T Deserialize<T>(string p_fullPathWithFileName, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		T v_desserializedObject = default(T);
		try
		{
			string v_fileText = ReadAllText(p_fullPathWithFileName);
			v_desserializedObject = DeserializeFromString<T>(v_fileText, p_serializationType);
		}
		catch {}
		
		return v_desserializedObject;
	}

	#endregion

	#region From/To PlayerPrefs

	public static bool SerializeToPlayerPrefs<T>(T p_object, string p_key, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		bool v_sucess = false;
		try
		{
			string v_stringObject = SerializeToString(p_object, p_serializationType);
			WriteAllTextToPlayerPrefs(v_stringObject, p_key);
		}
		catch {}
		
		return v_sucess;
	}

	public static T DeserializeFromPlayerPrefs<T>(string p_key, SerializationTypeEnum p_serializationType = SerializationTypeEnum.AutoDetect)
	{
		T v_desserializedObject = default(T);
		try
		{
			string v_fileText = ReadAllTextFromPlayerPrefs(p_key);
			v_desserializedObject = DeserializeFromString<T>(v_fileText, p_serializationType);
		}
		catch {}
		
		return v_desserializedObject;
	}

	#endregion
	
	#endregion

	#endregion
	
	#region Helper Functions

	#region To/From File

	public static bool WriteAllText(string p_textToWrite, string p_fullPathWithFileName)
	{
		bool v_sucess = false;
		if(!string.IsNullOrEmpty(p_textToWrite))
		{
			try
			{
				string v_fileNameWithoutExtension = KiltUtils.GetFileName(p_fullPathWithFileName, false);
				string v_folderPath = p_fullPathWithFileName.Replace(KiltUtils.GetFileName(p_fullPathWithFileName, true), "");
				string v_resourcesPath = KiltUtils.PathCombine(v_folderPath, v_fileNameWithoutExtension);
				v_resourcesPath = KiltUtils.PathUncombine(v_resourcesPath, KiltUtils.GetCurrentDataPath());
				#if (UNITY_WEBGL || UNITY_WEBPLAYER || UNITY_WINRT) && !UNITY_EDITOR
				v_sucess = WriteAllTextToPlayerPrefs(p_textToWrite, v_resourcesPath);
				#elif (UNITY_WEBGL || UNITY_WEBPLAYER) && UNITY_EDITOR
				try
				{
					KiltUtils.CallEditorStaticFunction("SystemIOEditorWebPlayer", "CreateDirectory", v_folderPath);
					KiltUtils.CallEditorStaticFunction("SystemIOEditorWebPlayer", "WriteAllText", p_fullPathWithFileName, p_textToWrite);
					v_sucess = true;
				}
				catch{}
				#elif !UNITY_WEBGL && !UNITY_WEBPLAYER
				try
				{
					//WP8 does not support WriteAllText
					System.IO.Directory.CreateDirectory(v_folderPath);
					using (var v_writer = new StreamWriter(p_fullPathWithFileName))
					{
						v_writer.Write(p_textToWrite);
						v_writer.Close();
					}
					v_sucess = true;
				}
				catch{}
				#endif

			}
			catch{}
		}
		return v_sucess;
	}

	public static bool WriteAllText(string p_textToWrite, string p_folder, string p_file, bool p_folderIsRelativePath = true)
	{
		string v_resultPath = KiltUtils.GetCompleteDataPath(p_folder, p_file, p_folderIsRelativePath);
		return WriteAllText(p_textToWrite, v_resultPath);
	}

	public static string ReadAllText(string p_fullPathWithFileName)
	{
		string v_return = "";
		try
		{
			string v_fileNameWithoutExtension = KiltUtils.GetFileName(p_fullPathWithFileName, false);
			string v_resultPath = p_fullPathWithFileName;
			string v_folderPath = v_resultPath.Replace(KiltUtils.GetFileName(p_fullPathWithFileName, true), "");
			string v_resourcesPath = KiltUtils.PathCombine(v_folderPath, v_fileNameWithoutExtension);
			v_resourcesPath = KiltUtils.PathUncombine(v_resourcesPath, KiltUtils.GetCurrentDataPath());

			//Try Resources.Load
			if(string.IsNullOrEmpty(v_return))
			{
				TextAsset v_textAsset = Resources.Load(v_resourcesPath, typeof(TextAsset)) as TextAsset;
				if(v_textAsset != null && !string.IsNullOrEmpty(v_textAsset.text))
					v_return = v_textAsset.text;;
			}
			//Try Traditional Methods
			#if (UNITY_WEBGL || UNITY_WEBPLAYER || UNITY_WINRT) && !UNITY_EDITOR
			if(string.IsNullOrEmpty(v_return))
			{
				string v_textObject = ReadAllTextFromPlayerPrefs(v_resourcesPath);
				if(!string.IsNullOrEmpty(v_textObject))
					v_return = v_textObject;
				
			}
			#elif (UNITY_WEBGL || UNITY_WEBPLAYER) && UNITY_EDITOR
			if(string.IsNullOrEmpty(v_return) && KiltUtils.CallEditorStaticFunctionWithReturn<bool>("SystemIOEditorWebPlayer", "FileExists", v_resultPath))
			{
				string v_textObject = KiltUtils.CallEditorStaticFunctionWithReturn<string>("SystemIOEditorWebPlayer", "ReadAllText", v_resultPath);
				if(!string.IsNullOrEmpty(v_textObject))
					v_return = v_textObject;
				
			}
			#elif !UNITY_WEBGL && !UNITY_WEBPLAYER
			if(string.IsNullOrEmpty(v_return) && System.IO.File.Exists(v_resultPath))
			{
				//WP8 does not support ReadAllText
				string v_textObject = "";
				try
				{
					using (var reader = new StreamReader(v_resultPath))
					{
						v_textObject = reader.ReadToEnd();
						reader.Close();
					}
				}
				catch{}
				if(!string.IsNullOrEmpty(v_textObject))
					v_return = v_textObject;
			}
			#endif
		}
		catch {}
		return v_return;
	}
	
	public static string ReadAllText(string p_folder, string p_file, bool p_folderIsRelativePath = true)
	{
		string v_resultPath = KiltUtils.GetCompleteDataPath(p_folder, p_file, p_folderIsRelativePath, false);
		return ReadAllText(v_resultPath);
	}

	#endregion

	#region To/From PlayerPrefs

	public static bool WriteAllTextToPlayerPrefs(string p_textToWrite, string p_key)
	{
		bool v_sucess = false;
		try
		{
			PlayerPrefs.SetString(p_key, p_textToWrite);
			PlayerPrefs.Save();
			v_sucess = true;
		}
		catch{}
		return v_sucess;
	}
	
	public static string ReadAllTextFromPlayerPrefs(string p_key)
	{
		string v_return = "";
		try
		{
			if(PlayerPrefs.HasKey(p_key))
				v_return = PlayerPrefs.GetString(p_key);
		}
		catch {}
		return v_return;
	}

	#endregion

	#region Encode/Decode

	public static string EncodeTo64(string p_toEncode)		
	{
		return EncodeTo64(p_toEncode, System.Text.Encoding.UTF8);
	}
	
	public static string EncodeTo64(string p_toEncode, System.Text.Encoding p_encoding)		
	{
		p_toEncode = p_toEncode == null? "" : p_toEncode;
		string v_returnValue = p_toEncode;
		try
		{
			byte[] v_toEncodeAsBytes = p_encoding.GetBytes(p_toEncode);
			if(p_encoding != null)
				v_returnValue = System.Convert.ToBase64String(v_toEncodeAsBytes);
		}
		catch{}
		return v_returnValue;
	}

	public static string EncodeTo64(byte[] p_data, System.Text.Encoding p_encoding)		
	{
		p_data = p_data == null? new byte[0] : p_data;
		string v_returnValue = "";
		try
		{
			if(p_encoding != null)
				v_returnValue = System.Convert.ToBase64String(p_data);
		}
		catch{}
		return v_returnValue;
	}
	
	public static string DecodeFrom64(string p_encodedData)	
	{
		return DecodeFrom64(p_encodedData, System.Text.Encoding.UTF8);
	}
	
	public static string DecodeFrom64(string p_encodedData, System.Text.Encoding p_encoding)	
	{
		p_encodedData = p_encodedData == null? "" : p_encodedData;
		string v_returnValue = p_encodedData;
		try
		{
			byte[] v_encodedDataAsBytes = System.Convert.FromBase64String(p_encodedData);
			if(p_encoding != null)
				v_returnValue = p_encoding.GetString(v_encodedDataAsBytes, 0, v_encodedDataAsBytes.Length);
		}
		catch{}
		return v_returnValue;
	}

	public static string DecodeFrom64(byte[] p_data, System.Text.Encoding p_encoding)	
	{
		p_data = p_data == null? new byte[0] : p_data;
		string v_returnValue = "";
		try
		{
			if(p_encoding != null)
				v_returnValue = p_encoding.GetString(p_data, 0, p_data.Length);
		}
		catch{}
		return v_returnValue;
	}

	public static byte[] GetBytesFromString(string p_string, System.Text.Encoding p_encoding)		
	{
		p_string = p_string == null? "" : p_string;
		byte[] v_returnValue = new byte[0];
		try
		{
			byte[] v_bytes = p_encoding.GetBytes(p_string);
			if(v_bytes != null)
				v_returnValue = v_bytes;
		}
		catch{}
		return v_returnValue;
	}

	public static byte[] GetBytesFromString(string p_string)	
	{
		return GetBytesFromString(p_string, System.Text.Encoding.UTF8);
	}

	public static string GetStringFromBytes(byte[] p_data, System.Text.Encoding p_encoding)		
	{
		p_data = p_data == null? new byte[0] : p_data;
		string v_returnValue = "";
		try
		{
			string v_string = p_encoding.GetString(p_data, 0, p_data.Length);
			if(v_string != null)
				v_returnValue = v_string;
		}
		catch{}
		return v_returnValue;
	}
	
	public static string GetStringFromBytes(byte[] p_data)	
	{
		return GetStringFromBytes(p_data, System.Text.Encoding.UTF8);
	}

	#endregion
	
	#endregion
}
