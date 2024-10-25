import SwiftUI

struct ChecklistView: View {
    @ObservedObject var viewModel: ProtocolViewModel
    
    var body: some View {
        ScrollViewReader { proxy in
            List {
                ForEach(Array(viewModel.checklistItems.enumerated()), id: \.element.id) { index, item in
                    CheckItemView(checklistItem: item)
                        .id(index)
                }
            }
            .onChange(of: viewModel.lastCheckedItemIndex) { _, newIndex in
                if let nextUncheckedIndex = viewModel.checklistItems.firstIndex(where: { !$0.isChecked }) {
                    withAnimation {
                        proxy.scrollTo(nextUncheckedIndex, anchor: .center)
                    }
                }
            }
        }
    }
}
