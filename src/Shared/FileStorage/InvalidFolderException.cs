// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     InvalidFolderException.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.FileStorage;

public class InvalidFolderException : Exception
{

	public InvalidFolderException() : base("Invalid folder location.") { }

	public InvalidFolderException(string message) : base(message) { }

	public InvalidFolderException(string message, Exception innerException) : base(message, innerException) { }

}
