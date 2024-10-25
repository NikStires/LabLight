import SwiftUI
import UnityFramework

struct ProtocolView: View {
    @StateObject private var viewModel: ProtocolViewModel
    
    init(selectedProtocol: ProtocolDefinition) {
        _viewModel = StateObject(wrappedValue: ProtocolViewModel(selectedProtocol: selectedProtocol))
    }
    
    var body: some View {
        VStack(spacing: 16) {
            Picker("Select Step", selection: $viewModel.selectedStepIndex) {
                ForEach(0..<viewModel.selectedProtocol.steps.count, id: \.self) { index in
                    Text("Step \(index + 1)").tag(index)
                }
            }
            .pickerStyle(SegmentedPickerStyle())
            .padding(.horizontal)
            
            HStack(spacing: 0) {
                ChecklistView(viewModel: viewModel)
                    .frame(maxWidth: viewModel.currentStep.contentItems.isEmpty ? .infinity : 400)
                    .animation(.spring(), value: viewModel.currentStep.contentItems.isEmpty)
                
                if !viewModel.currentStep.contentItems.isEmpty {
                    ProtocolContentView(contentItems: viewModel.currentStep.contentItems)
                        .frame(maxWidth: .infinity)
                        .padding(.leading, 10)
                        .transition(.move(edge: .trailing))
                }
            }
            .padding(.horizontal)
        }
        .navigationBarTitleDisplayMode(.inline)
        .navigationTitle(viewModel.selectedProtocol.title)
        .ornament(visibility: .visible, attachmentAnchor: .scene(.leading)) {
            VStack(spacing: 20) {
                Button(action: viewModel.checkNextItem) {
                    Image(systemName: "checkmark")
                }
                .disabled(viewModel.nextUncheckedItem() == nil)
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4,trailing: 8))
                
                Button(action: viewModel.uncheckLastItem) {
                    Image(systemName: "xmark")
                }
                .disabled(viewModel.lastCheckedItem() == nil)
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4,trailing: 8))
                
                Button(action: viewModel.goToPreviousStep) {
                    Image(systemName: "chevron.left")
                }
                .disabled(viewModel.selectedStepIndex == 0)
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4,trailing: 8))

                Button(action: viewModel.goToNextStep) {
                    Image(systemName: "chevron.right")
                }
                .disabled(viewModel.selectedStepIndex >= viewModel.selectedProtocol.steps.count - 1)
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4,trailing: 8))

                Button(action: viewModel.openPDF) {
                    Image(systemName: "doc.richtext")
                }
                .disabled(viewModel.selectedProtocol.pdfPath == nil)
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4,trailing: 8))

            }
            .padding()
            .buttonStyle(.plain)
            .glassBackgroundEffect(in: RoundedRectangle(cornerRadius: 22))
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

    @Published var lastCheckedItemIndex: Int?

    init(selectedProtocol: ProtocolDefinition) {
        self.selectedProtocol = selectedProtocol
        self.checklistItems = selectedProtocol.steps.first?.checklist ?? []
        NotificationCenter.default.addObserver(self, selector: #selector(handleStepChange(_:)), name: Notification.Name("StepChange"), object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(handleCheckItemChange(_:)), name: Notification.Name("CheckItemChange"), object: nil)
    }

    @objc func handleStepChange(_ notification: Notification) {
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("stepChange:") {
            let stepStateData = try? JSONDecoder().decode(StepStateData.self, from: message.dropFirst("stepChange:".count).data(using: .utf8)!)
            if let stepStateData = stepStateData {
                selectedStepIndex = stepStateData.currentStepIndex
                updateChecklistItemStates(stepStateData.checklistState)
            } else {
                print("Invalid stepChange message format")
            }
        }
    }

    @objc func handleCheckItemChange(_ notification: Notification) {
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("checkItemChange:") {
            let checkItemStateDataList = try? JSONDecoder().decode([CheckItemStateData].self, from: message.dropFirst("checkItemChange:".count).data(using: .utf8)!)
            if let checkItemStateDataList = checkItemStateDataList {
                updateChecklistItemStates(checkItemStateDataList)
            } else {
                print("Invalid checkItemChange message format")
            }
        }
    }

    func updateChecklistItemStates(_ checklistState: [CheckItemStateData]?) {
        checklistItems = currentStep.checklist
        for checkItemState in checklistState ?? [] {
            updateCheckItemState(checkItemState)
        }
    }

    func updateCheckItemState(_ checkItemState: CheckItemStateData) {
        print("######LABLIGHT Updating checklist item \(checkItemState.checkIndex) to \(checkItemState.isChecked)")
        checklistItems[checkItemState.checkIndex].isChecked = checkItemState.isChecked
        if checkItemState.isChecked {
            lastCheckedItemIndex = checkItemState.checkIndex
        }
    }

    var currentStep: Step {
        selectedProtocol.steps[selectedStepIndex]
    }

    func checkNextItem() {
        guard let nextItemIndex = checklistItems.firstIndex(where: { !$0.isChecked }) else { return }
        CallCSharpCallback("checkItem:" + String(nextItemIndex))
    }

    func uncheckLastItem() {
        guard let lastItemIndex = checklistItems.lastIndex(where: { $0.isChecked }) else { return }
        CallCSharpCallback("uncheckItem:" + String(lastItemIndex))
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
        CallCSharpCallback("requestPDF:")
    }

    func nextUncheckedItem() -> ChecklistItem? {
        return checklistItems.first(where: { !$0.isChecked })
    }

    func lastCheckedItem() -> ChecklistItem? {
        return checklistItems.last(where: { $0.isChecked })
    }
}

struct StepStateData: Codable {
    let currentStepIndex: Int
    let isSignedOff: Bool
    let checklistState: [CheckItemStateData]?
    
    enum CodingKeys: String, CodingKey {
        case currentStepIndex = "CurrentStepIndex"
        case isSignedOff = "IsSignedOff"
        case checklistState = "Checklist"
    }
}

struct CheckItemStateData: Codable {
    let isChecked: Bool
    let checkIndex: Int
    
    enum CodingKeys: String, CodingKey {
        case isChecked = "IsChecked"
        case checkIndex = "CheckIndex"
    }
}
