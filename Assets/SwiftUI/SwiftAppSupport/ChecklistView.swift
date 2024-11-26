import SwiftUI

struct ChecklistView: View {
    @ObservedObject var viewModel: ProtocolViewModel
    
    var body: some View {
        VStack(spacing: 8) {
            // Sign-off status header
            HStack {
                Image(systemName: viewModel.isStepSignedOff ? 
                    "checkmark.seal.fill" : "checkmark.seal")
                    .foregroundColor(viewModel.isStepSignedOff ? 
                        .green : .gray)
                
                Text(viewModel.isStepSignedOff ? 
                    "Step Signed Off" : "Step Not Signed Off")
                    .font(.subheadline)
                    .foregroundColor(viewModel.isStepSignedOff ? 
                        .green : .gray)
            }
            .padding(.horizontal)
            .padding(.top, 8)
            
            // Existing checklist
            ScrollViewReader { proxy in
                List {
                    ForEach(viewModel.checklistItems) { item in
                        CheckItemView(
                            definition: item,
                            isChecked: viewModel.currentStates.first { 
                                $0.checkIndex == viewModel.getIndex(for: item) 
                            }?.isChecked ?? false
                        )
                        .id(viewModel.getIndex(for: item))
                        .disabled(viewModel.isStepSignedOff) // Disable interaction when signed off
                    }
                }
                .onChange(of: viewModel.lastCheckedItemIndex) { _, newIndex in
                    if let nextUncheckedIndex = viewModel.checklistItems.indices.first(where: { index in
                        !(viewModel.currentStates.first { $0.checkIndex == index }?.isChecked ?? false)
                    }) {
                        withAnimation {
                            proxy.scrollTo(nextUncheckedIndex, anchor: .center)
                        }
                    }
                }
            }
        }
    }
}
