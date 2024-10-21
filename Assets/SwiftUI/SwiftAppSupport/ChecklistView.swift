import SwiftUI

struct ChecklistView: View {
    let checklistItems: [ChecklistItem]
    
    var body: some View {
        List {
            ForEach(checklistItems) { item in
                CheckItemView(checklistItem: item)
            }
        }
    }
}
