using System;
using System.Drawing;

using Sulakore.Components;

namespace Sulakore.Extensions
{
    public interface IExtension
    {
        /// <summary>
        /// Gets a value indicating whether the extension is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the name of the extension given by the Author.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the name(s) of the developer(s) that wrote the extension.
        /// </summary>
        string Author { get; }
        /// <summary>
        /// Gets the file location of the extension from which the instance was initialized from.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Gets the logo of the extension.
        /// </summary>
        Bitmap Logo { get; }
        /// <summary>
        /// Gets the assembly version of the extension.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the IContractor instance used for communication between the initializer.
        /// </summary>
        IContractor Contractor { get; }
        /// <summary>
        /// Gets or sets the priorty of the extension that determines whether a new thread is spawned when handling the flow of data(High), or whether to pull one from the system's thread pool(Normal).
        /// </summary>
        ExtensionPriority Priority { get; set; }

        /// <summary>
        /// Gets the Type found in the extension's project scope that inherits from SKoreExtensionForm.
        /// </summary>
        Type UIContextType { get; }
        /// <summary>
        /// Gets the Form that represents the extension's main GUI.
        /// </summary>
        SKoreExtensionForm UIContext { get; }

        /// <summary>
        /// Attempts to invoke a custom command with arguments.
        /// </summary>
        /// <param name="invoker">The source of the method call.</param>
        /// <param name="command">The command for the invokee to utilize.</param>
        /// <param name="args">The arguments given to be used by the invokee to process the command.</param>
        /// <returns></returns>
        object Invoke(object invoker, string command, params object[] arg);
    }
}