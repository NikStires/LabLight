import Foundation
import SwiftUI
import PDFKit

// MARK: - Callback Delegate

typealias CallbackDelegateType = @convention(c) (UnsafePointer<CChar>) -> Void

private var sCallbackDelegate: CallbackDelegateType? = nil

// Make this variable public and accessible globally
public var CallCSharpCallback: (String) -> Void = { str in
    str.withCString { sCallbackDelegate?($0) }
}

@_cdecl("SetNativeCallback")
func setNativeCallback(_ delegate: CallbackDelegateType) {
    sCallbackDelegate = delegate
    CallCSharpCallback = { str in
        str.withCString { sCallbackDelegate?($0) }
    }
}

public func CallCSharpCallback(_ str: String) {
    if let callback = sCallbackDelegate {
        str.withCString { callback($0) }
    }
}

// MARK: - Window Management

@_cdecl("OpenSwiftUIWindow")
func openSwiftUIWindow(_ cname: UnsafePointer<CChar>) {
    let openWindow = EnvironmentValues().openWindow
    let name = String(cString: cname)
    openWindow(id: name)
}

@_cdecl("CloseSwiftUIWindow")
func closeSwiftUIWindow(_ cname: UnsafePointer<CChar>) {
    let dismissWindow = EnvironmentValues().dismissWindow
    let name = String(cString: cname)
    dismissWindow(id: name)
}

// MARK: - PDF Functionality

@_cdecl("OpenSwiftPdfWindow")
func openSwiftPdfWindow(_ cname: UnsafePointer<CChar>) {
    let pdfString = String(cString: cname)
    let openWindow = EnvironmentValues().openWindow
    openWindow(id: "PDF", value: pdfString)
}

// MARK: - Video Functionality

@_cdecl("OpenSwiftVideoWindow")
func openSwiftVideoWindow(_ cname: UnsafePointer<CChar>) {
    let videoTitle = String(cString: cname)
    let openWindow = EnvironmentValues().openWindow
    openWindow(id: "Video", value: videoTitle)
}

// MARK: - Safari Functionality

@_cdecl("OpenSwiftSafariWindow")
func openSwiftSafariWindow(_ cname: UnsafePointer<CChar>) {
    let urlString = String(cString: cname)
    let openWindow = EnvironmentValues().openWindow
    openWindow(id: "Safari", value: urlString)
}

// MARK: - LLM Chat Functionality

@_cdecl("SendMessageToSwiftUI")
func sendMessageToSwiftUI(_ cmessage: UnsafePointer<CChar>) {
    let message = String(cString: cmessage)
    NotificationCenter.default.post(name: Notification.Name("LLMChatMessage"), object: nil, userInfo: ["message": message])
}

// MARK: - Timer Functionality

@_cdecl("OpenSwiftTimerWindow")
func openSwiftTimerWindow(_ duration: Int32) {
    let openWindow = EnvironmentValues().openWindow
    openWindow(id: "Timer", value: Int(duration))
}

// MARK: - Scientific Calculator Functionality

@_cdecl("OpenSwiftCalculatorWindow")
func openSwiftScientificCalculatorWindow() {
    let openWindow = EnvironmentValues().openWindow
    openWindow(id: "Calculator")
}

// MARK: - Any additional functionality...

// You can add more sections here for other plugin functionalities
