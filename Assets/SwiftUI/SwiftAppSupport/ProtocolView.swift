import SwiftUI

struct ProtocolView: View {
    let selectedProtocol: ProtocolDefinition
    
    @State private var selectedStepIndex: Int = 0
    
    var body: some View {
        VStack {
            Picker("Select Step", selection: $selectedStepIndex) {
                ForEach(0..<selectedProtocol.steps.count, id: \.self) { index in
                    Text("Step \(index + 1)").tag(index)
                }
            }
            .pickerStyle(SegmentedPickerStyle())
            .padding()
            
            HStack(spacing: 0) {
                ChecklistView(checklistItems: currentStep.checklist)
                    .frame(maxWidth: .infinity)
                
                ProtocolContentView(contentItems: currentStep.contentItems)
                    .frame(maxWidth: .infinity)
                    .padding(.leading, 10)
            }
        }
        .navigationTitle(selectedProtocol.title)
        .padding()
        .ornament(visibility: .visible, attachmentAnchor: .scene(.leading)) {
            VStack(spacing: 20) {
                Button(action: checkNextItem) {
                    Image(systemName: "checkmark")
                }
                .disabled(nextUncheckedItem() == nil)
                
                Button(action: uncheckLastItem) {
                    Image(systemName: "xmark")
                }
                .disabled(lastCheckedItem() == nil)
                
                Button(action: goToPreviousStep) {
                    Image(systemName: "chevron.left")
                }
                .disabled(selectedStepIndex == 0)
                
                Button(action: goToNextStep) {
                    Image(systemName: "chevron.right")
                }
                .disabled(selectedStepIndex >= selectedProtocol.steps.count - 1)
                
                Button(action: openPDF) {
                    Image(systemName: "doc.richtext")
                }
                .disabled(selectedProtocol.pdfPath == nil)
            }
            .padding()
            .frame(width: 100)
        }
    }
    
    private var currentStep: Step {
        selectedProtocol.steps[selectedStepIndex]
    }
    
    // MARK: - Action Methods
    
    private func checkNextItem() {
        guard let nextItem = currentStep.checklist.first(where: { !$0.isChecked }) else { return }
        nextItem.isChecked = true
    }
    
    private func uncheckLastItem() {
        guard let lastItem = currentStep.checklist.reversed().first(where: { $0.isChecked }) else { return }
        lastItem.isChecked = false
    }
    
    private func goToNextStep() {
        if selectedStepIndex < selectedProtocol.steps.count - 1 {
            selectedStepIndex += 1
        }
    }
    
    private func goToPreviousStep() {
        if selectedStepIndex > 0 {
            selectedStepIndex -= 1
        }
    }
    
    private func openPDF() {
        let openWindow = EnvironmentValues().openWindow
        openWindow(id: "PDF", value: selectedProtocol.pdfPath)
    }
    
    private func nextUncheckedItem() -> ChecklistItem? {
        return selectedProtocol.steps[selectedStepIndex].checklist.first(where: { !$0.isChecked })
    }
    
    private func lastCheckedItem() -> ChecklistItem? {
        return selectedProtocol.steps[selectedStepIndex].checklist.reversed().first(where: { $0.isChecked })
    }
}
