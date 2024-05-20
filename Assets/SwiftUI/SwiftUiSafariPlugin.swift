//
// This is a Swift plugin that provides an interface for
// SwiftUI to interact with. It must be linked into
// UnityFramework, which is what the default Swift file plugin
// importer will place it into.
//
// It uses "@_cdecl", a Swift not-officially-supported attribute to
// provide C-style linkage and symbol for a given function.
//
// It also uses a "hack" to create an EnvironmentValues() instance
// in order to fetch the openWindow and dismissWindow action. Normally,
// these would be provided to a view via something like:
//
//    @Environment(\.openWindow) var openWindow
//
// but we don't have a view at this point, and it's expected that these
// actions will be global (and not view-specific) anyway.
//
// There are two additional files that complete this operation:
// SwiftUIInjectedScene.swift and PDFConventView.swift.
//
// Any file named "...InjectedScene.swift" will be moved to the Unity-VisionOS
// Xcode target (as it must be there in order to be referenced by the App), and
// its static ".scene" member will be added to the App's main Scene. See
// the comments in SwiftUIInjectedScene.swift for more information.
//
// Any file that's inside of a "SwiftAppSupport" directory anywhere in its path
// will also be moved to the Unity-VisionOS Xcode target. PDFContentView.swift
// is inside SwiftAppSupport beceause it's needed by the WindowGroup this sample
// adds to provide its content.
//

import Foundation
import SwiftUI

// These methods are exported from Swift with an explicit C-style name using @_cdecl,
// to match what DllImport expects. You will need to do appropriate conversion from
// C-style argument types (including UnsafePointers and other friends) into Swift
// as appropriate.

// Declared in C# as: static extern void OpenSwiftPdfWindow(string urlString);
@_cdecl("OpenSwiftSafariWindow")
func openSwiftSafariWindow(_ cname: UnsafePointer<CChar>)
{
    let urlString = String(cString: cname)
    print("############ OPEN Safari \(urlString)")

    let openWindow = EnvironmentValues().openWindow
    openWindow(id: "Safari", value: urlString)
}
