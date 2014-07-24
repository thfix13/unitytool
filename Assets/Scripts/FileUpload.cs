// C# file names: "FileUpload.cs"
using System.IO;
using UnityEngine;
using System.Collections;

public class FileUpload
{
	private string m_LocalFileName = "playerPath.XML";
	private string m_URL = "http://cgi.cs.mcgill.ca/~ptorre2/upload_file.php";

	public void UploadFile()
	{
		WWW localFile = new WWW("file:///" + Path.GetFullPath(".") + "/" + m_LocalFileName);

		while (!localFile.isDone) {}

		if (localFile.error == null)
			Debug.Log("Loaded file successfully");
		else
		{
			Debug.Log("Open file error: "+localFile.error);
			return;
		}
		
		WWWForm postForm = new WWWForm();
		// version 1
		//postForm.AddBinaryData("theFile",localFile.bytes);
		
		// version 2
		postForm.AddBinaryData("file",localFile.bytes, m_LocalFileName, "text/plain");
		
		WWW upload = new WWW(m_URL,postForm);

		while (!upload.isDone) {}

		if (upload.error == null)
			Debug.Log("upload done :" + upload.text);
		else
			Debug.Log("Error during upload: " + upload.error);

	}
}