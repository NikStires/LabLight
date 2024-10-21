import SwiftUI

struct CheckItemView: View {
    @ObservedObject var checklistItem: ChecklistItem
    
    var body: some View {
        HStack {
            Toggle(isOn: $checklistItem.isChecked) {
                Text(checklistItem.text)
                    .strikethrough(checklistItem.isChecked, color: .white)
                    .foregroundColor(checklistItem.isChecked ? .gray : .white)
            }
            .toggleStyle(CheckBoxToggleStyle())
        }
    }
}

// Custom Toggle Style to resemble a Checkbox
struct CheckBoxToggleStyle: ToggleStyle {
    func makeBody(configuration: Configuration) -> some View {
        Button(action: { configuration.isOn.toggle() }) {
            HStack {
                Image(systemName: configuration.isOn ? "checkmark.square" : "square")
                    .foregroundColor(configuration.isOn ? .blue : .gray)
                configuration.label
            }
        }
        .buttonStyle(PlainButtonStyle())
    }
}
