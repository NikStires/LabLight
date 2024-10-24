import SwiftUI
import UnityFramework

struct ProtocolView: View {
    @StateObject private var viewModel: ProtocolViewModel
    
    init(selectedProtocol: ProtocolDefinition) {
        _viewModel = StateObject(wrappedValue: ProtocolViewModel(selectedProtocol: selectedProtocol))
    }
    
    var body: some View {
        VStack {
            Picker("Select Step", selection: $viewModel.selectedStepIndex) {
                ForEach(0..<viewModel.selectedProtocol.steps.count, id: \.self) { index in
                    Text("Step \(index + 1)").tag(index)
                }
            }
            .pickerStyle(SegmentedPickerStyle())
            .padding()
            
            HStack(spacing: 0) {
                ChecklistView(checklistItems: viewModel.checklistItems)
                    .frame(maxWidth: .infinity)
                
                ProtocolContentView(contentItems: viewModel.currentStep.contentItems)
                    .frame(maxWidth: .infinity)
                    .padding(.leading, 10)
            }
        }
        .navigationTitle(viewModel.selectedProtocol.title)
        .padding()
        .ornament(visibility: .visible, attachmentAnchor: .scene(.leading)) {
            VStack(spacing: 20) {
                Button(action: viewModel.checkNextItem) {
                    Image(systemName: "checkmark")
                }
                .disabled(viewModel.nextUncheckedItem() == nil)
                
                Button(action: viewModel.uncheckLastItem) {
                    Image(systemName: "xmark")
                }
                .disabled(viewModel.lastCheckedItem() == nil)
                
                Button(action: viewModel.goToPreviousStep) {
                    Image(systemName: "chevron.left")
                }
                .disabled(viewModel.selectedStepIndex == 0)
                
                Button(action: viewModel.goToNextStep) {
                    Image(systemName: "chevron.right")
                }
                .disabled(viewModel.selectedStepIndex >= viewModel.selectedProtocol.steps.count - 1)
                
                Button(action: viewModel.openPDF) {
                    Image(systemName: "doc.richtext")
                }
                .disabled(viewModel.selectedProtocol.pdfPath == nil)
            }
            .padding()
            .frame(width: 100)
        }
    }
}
    
class ProtocolViewModel: ObservableObject {
    let selectedProtocol: ProtocolDefinition

    @Published var selectedStepIndex: Int = 0 {
        didSet {
            if selectedStepIndex != oldValue {
                CallCSharpCallback("stepNavigation:" + String(selectedStepIndex))
            }
        }
    }
    @Published var checklistItems: [ChecklistItem] = []

    init(selectedProtocol: ProtocolDefinition) {
        self.selectedProtocol = selectedProtocol
        self.checklistItems = selectedProtocol.steps.first?.checklist ?? []
        NotificationCenter.default.addObserver(self, selector: #selector(handleStepChange(_:)), name: Notification.Name("StepChange"), object: nil)
    }

    @objc func handleStepChange(_ notification: Notification) {
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("stepChange:") {
            let parts = message.dropFirst("stepChange:".count).split(separator: ":")
            if parts.count == 2, let stepIndex = Int(parts[0]){
                selectedStepIndex = stepIndex
                checklistItems = currentStep.checklist
            } else {
                print("Invalid stepChange message format")
            }
        }
    }

    var currentStep: Step {
        selectedProtocol.steps[selectedStepIndex]
    }

    func checkNextItem() {
        guard let nextItemIndex = checklistItems.firstIndex(where: { !$0.isChecked }) else { return }
        checklistItems[nextItemIndex].isChecked.toggle()
    }

    func uncheckLastItem() {
        guard let lastItemIndex = checklistItems.lastIndex(where: { $0.isChecked }) else { return }
        checklistItems[lastItemIndex].isChecked.toggle()
    }

    func goToNextStep() {
        if selectedStepIndex < selectedProtocol.steps.count - 1 {
            selectedStepIndex += 1
        }
    }

    func goToPreviousStep() {
        if selectedStepIndex > 0 {
            selectedStepIndex -= 1
        }
    }

    func openPDF() {
        let openWindow = EnvironmentValues().openWindow
        openWindow(id: "PDF", value: selectedProtocol.pdfPath)
    }

    func nextUncheckedItem() -> ChecklistItem? {
        return checklistItems.first(where: { !$0.isChecked })
    }

    func lastCheckedItem() -> ChecklistItem? {
        return checklistItems.last(where: { $0.isChecked })
    }
}
