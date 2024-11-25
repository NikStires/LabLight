import SwiftUI
import UnityFramework

struct ProtocolView: View {
    // MARK: - Properties
    @StateObject private var viewModel: ProtocolViewModel
    @Namespace private var animation
    @State private var showingPDFMenu = false
    
    // MARK: - Initialization
    init(selectedProtocol: ProtocolDefinition) {
        _viewModel = StateObject(wrappedValue: ProtocolViewModel(selectedProtocol: selectedProtocol))
    }
    
    // MARK: - Body
    var body: some View {
        VStack(spacing: 16) {
            stepPicker
            
            contentArea
        }
        .animation(.spring(response: 0.3, dampingFraction: 0.8), value: viewModel.currentStep.contentItems.isEmpty)
        .navigationBarTitleDisplayMode(.inline)
        .navigationTitle(viewModel.selectedProtocol.title)
        .ornament(visibility: .visible, attachmentAnchor: .scene(.leading)) {
            controlPanel
        }
    }
    
    // MARK: - View Components
    private var stepPicker: some View {
        Picker("Select Step", selection: $viewModel.selectedStepIndex) {
            ForEach(0..<viewModel.selectedProtocol.steps.count, id: \.self) { index in
                Text("Step \(index + 1)").tag(index)
            }
        }
        .pickerStyle(SegmentedPickerStyle())
        .padding(.horizontal)
    }
    
    private var contentArea: some View {
        HStack(spacing: 0) {
            ChecklistView(viewModel: viewModel)
                .frame(maxWidth: viewModel.currentStep.contentItems.isEmpty ? .infinity : 400)
                .animation(.spring(response: 0.3, dampingFraction: 0.8), value: viewModel.currentStep.contentItems.isEmpty)
            
            if !viewModel.currentStep.contentItems.isEmpty {
                ProtocolContentView(
                    contentItems: viewModel.currentStep.contentItems,
                    selectedChecklistItem: viewModel.selectedChecklistItem
                )
                .frame(maxWidth: .infinity)
                .padding(.leading, 10)
                .transition(.asymmetric(
                    insertion: .opacity.combined(with: .move(edge: .trailing)),
                    removal: .opacity.combined(with: .move(edge: .trailing))
                ))
            }
        }
        .padding(.horizontal)
    }
    
    private var controlPanel: some View {
        VStack(spacing: 20) {
            checkButton
            uncheckButton
            navigationButtons
            pdfButton
        }
        .padding()
        .buttonStyle(.plain)
        .glassBackgroundEffect(in: RoundedRectangle(cornerRadius: 22))
    }
    
    private var checkButton: some View {
        Button(action: viewModel.checkNextItem) {
            Image(systemName: "checkmark")
        }
        .disabled(viewModel.nextUncheckedItem() == nil)
        .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 8))
    }
    
    private var uncheckButton: some View {
        Button(action: viewModel.uncheckLastItem) {
            Image(systemName: "xmark")
        }
        .disabled(viewModel.lastCheckedItem() == nil)
        .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 8))
    }
    
    private var navigationButtons: some View {
        Group {
            Button(action: viewModel.goToNextStep) {
                Image(systemName: "chevron.right")
            }
            .disabled(viewModel.selectedStepIndex >= viewModel.selectedProtocol.steps.count - 1)
            .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 8))
            
            Button(action: viewModel.goToPreviousStep) {
                Image(systemName: "chevron.left")
            }
            .disabled(viewModel.selectedStepIndex == 0)
            .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 8))
        }
    }
    
    private var pdfButton: some View {
        Group {
            if !viewModel.selectedProtocol.protocolPDFNames.isEmpty {
                Menu {
                    ForEach(viewModel.selectedProtocol.protocolPDFNames, id: \.self) { pdfName in
                        Button(action: { viewModel.openPDF(pdfName) }) {
                            Label(pdfName, systemImage: "doc.richtext")
                        }
                    }
                } label: {
                    Image(systemName: "doc.richtext")
                        .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 8))
                }
            }
        }
    }
}

// MARK: - ViewModel
class ProtocolViewModel: ObservableObject {
    // MARK: - Properties
    let selectedProtocol: ProtocolDefinition
    @Published var selectedStepIndex: Int = 0 {
        didSet {
            if selectedStepIndex != oldValue {
                CallCSharpCallback("stepNavigation|" + String(selectedStepIndex))
            }
        }
    }
    @Published var checklistItems: [CheckItemDefinition] = []
    @Published var currentStates: [CheckItemStateData] = []
    @Published var lastCheckedItemIndex: Int?
    @Published var selectedChecklistItem: CheckItemDefinition?
    
    // MARK: - Initialization
    init(selectedProtocol: ProtocolDefinition) {
        self.selectedProtocol = selectedProtocol
        self.checklistItems = selectedProtocol.steps.first?.checklist ?? []
        setupNotifications()
    }
    
    // MARK: - Setup
    private func setupNotifications() {
        NotificationCenter.default.addObserver(
            self,
            selector: #selector(handleStepChange(_:)),
            name: Notification.Name("StepChange"),
            object: nil
        )
        NotificationCenter.default.addObserver(
            self,
            selector: #selector(handleCheckItemChange(_:)),
            name: Notification.Name("CheckItemChange"),
            object: nil
        )
    }
    
    // MARK: - Computed Properties
    var currentStep: StepDefinition {
        selectedProtocol.steps[selectedStepIndex]
    }
    
    // MARK: - Public Methods
    func getItemState(for item: CheckItemDefinition) -> CheckItemStateData {
        let index = getIndex(for: item)
        let isChecked = currentStates.first { $0.checkIndex == index }?.isChecked ?? false
        return CheckItemStateData(isChecked: isChecked, checkIndex: index)
    }
    
    func getIndex(for item: CheckItemDefinition) -> Int {
        checklistItems.firstIndex(of: item) ?? 0
    }
    
    func nextUncheckedItem() -> CheckItemDefinition? {
        return checklistItems.enumerated()
            .first { index, _ in 
                !(currentStates.first { $0.checkIndex == index }?.isChecked ?? false)
            }?.element
    }

    func lastCheckedItem() -> CheckItemDefinition? {
        return checklistItems.enumerated()
            .filter { index, _ in 
                currentStates.first { $0.checkIndex == index }?.isChecked ?? false
            }
            .last?.element
    }
    
    // MARK: - Actions
    func checkNextItem() {
        guard let nextItem = nextUncheckedItem() else { return }
        CallCSharpCallback("checkItem|" + String(getIndex(for: nextItem)))
    }

    func uncheckLastItem() {
        guard let lastItem = lastCheckedItem() else { return }
        CallCSharpCallback("uncheckItem|" + String(getIndex(for: lastItem)))
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

    func openPDF(_ pdfName: String) {
        CallCSharpCallback("requestPDF|" + pdfName)
    }
    
    // MARK: - Notification Handlers
    @objc func handleStepChange(_ notification: Notification) {
        guard let message = notification.userInfo?["message"] as? String,
              message.hasPrefix("stepChange|"),
              let data = message.dropFirst("stepChange|".count).data(using: .utf8),
              let stepStateData = try? JSONDecoder().decode(StepStateData.self, from: data)
        else {
            print("Invalid step change message format")
            return
        }
        
        selectedStepIndex = stepStateData.currentStepIndex
        checklistItems = currentStep.checklist
        currentStates = stepStateData.checklistState ?? []
        lastCheckedItemIndex = currentStates.last { $0.isChecked }?.checkIndex
        selectedChecklistItem = nil
    }

    @objc func handleCheckItemChange(_ notification: Notification) {
        guard let message = notification.userInfo?["message"] as? String,
              message.hasPrefix("checkItemChange|"),
              let data = message.dropFirst("checkItemChange|".count).data(using: .utf8),
              let checkItemStateDataList = try? JSONDecoder().decode([CheckItemStateData].self, from: data)
        else {
            print("Invalid check item change message format")
            return
        }
        
        currentStates = checkItemStateDataList
        lastCheckedItemIndex = checkItemStateDataList.last { $0.isChecked }?.checkIndex
        
        if let lastIndex = lastCheckedItemIndex,
           lastIndex < checklistItems.count {
            selectedChecklistItem = checklistItems[lastIndex]
        }
    }
}

// MARK: - Models
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
