import SwiftUI

struct ProtocolOrnamentView: View {
    @Binding var selectedStepIndex: Int
    let protocolDefinition: ProtocolDefinition
    let checklistItems: [ChecklistItem]
    
    var onCheckNext: () -> Void
    var onUncheckLast: () -> Void
    var onNextStep: () -> Void
    var onPreviousStep: () -> Void
    var onOpenPDF: () -> Void
    
    var body: some View {
        VStack(spacing: 20) {
            Button(action: onCheckNext) {
                VStack {
                    Image(systemName: "checkmark")
                    Text("Check")
                }
            }
            .buttonStyle(OrnamentButtonStyle())
            .disabled(nextUncheckedItem() == nil)
            
            Button(action: onUncheckLast) {
                VStack {
                    Image(systemName: "xmark")
                    Text("Uncheck")
                }
            }
            .buttonStyle(OrnamentButtonStyle())
            .disabled(lastCheckedItem() == nil)
            
            Button(action: onPreviousStep) {
                VStack {
                    Image(systemName: "chevron.left")
                    Text("Previous")
                }
            }
            .buttonStyle(OrnamentButtonStyle())
            .disabled(selectedStepIndex == 0)
            
            Button(action: onNextStep) {
                VStack {
                    Image(systemName: "chevron.right")
                    Text("Next")
                }
            }
            .buttonStyle(OrnamentButtonStyle())
            .disabled(selectedStepIndex >= protocolDefinition.steps.count - 1)
            
            if protocolDefinition.pdfPath != nil {
                Button(action: onOpenPDF) {
                    VStack {
                        Image(systemName: "doc.richtext")
                        Text("Open PDF")
                    }
                }
                .buttonStyle(OrnamentButtonStyle())
            }
        }
        .padding()
        .frame(width: 100)
    }
    
    private func nextUncheckedItem() -> ChecklistItem? {
        return checklistItems.first(where: { !$0.isChecked })
    }
    
    private func lastCheckedItem() -> ChecklistItem? {
        return checklistItems.reversed().first(where: { $0.isChecked })
    }
}

struct OrnamentButtonStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        VStack {
            configuration.label
        }
        .padding()
        .background(Color.gray.opacity(0.2))
        .cornerRadius(8)
        .scaleEffect(configuration.isPressed ? 0.95 : 1.0)
    }
}
