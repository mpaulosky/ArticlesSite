// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FileData.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.FileStorage;

public record FileData(Stream File, FileMetaData Metadata)
{

	/// <summary>
	///   A placeholder for a missing file
	/// </summary>
	public static FileData Missing =>
			new(Stream.Null, new FileMetaData(string.Empty, string.Empty, DateTimeOffset.MinValue));

}

public record FileMetaData(string FileName, string ContentType, DateTimeOffset CreateDate)
{

	public void ValidateFileName()
	{
		if (string.IsNullOrEmpty(FileName))
		{
			throw new ArgumentException("Missing file name", nameof(FileName));
		}

		// run a regular expression check to ensure the file name is valid - no slashes or other special characters
		Regex reValidFileName = new (@"^[a-zA-Z0-9_\-\.]+$");

		if (!reValidFileName.IsMatch(FileName))
		{
			throw new ArgumentException("Invalid file name", nameof(FileName));
		}

	}

}