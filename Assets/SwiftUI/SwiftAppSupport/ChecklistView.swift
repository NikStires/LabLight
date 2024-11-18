import SwiftUI

struct ChecklistView: View {
    @ObservedObject var viewModel: ProtocolViewModel
    
    var body: some View {
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
