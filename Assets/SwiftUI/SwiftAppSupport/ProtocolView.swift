import SwiftUI

struct ProtocolView: View {
    let selectedProtocol: ProtocolDefinition
    
    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            Text(selectedProtocol.title)
                .font(.largeTitle)
                .padding(.top)
            Spacer()
        }
        .padding()
        .navigationTitle("Protocol Details")
    }
    
    func formatText(_ text: String) -> String {
        // Remove special characters
        var formatted = text.components(separatedBy: CharacterSet.alphanumerics.inverted).joined()
        
        // Add spaces before capital letters (for camel case)
        formatted = formatted.replacingOccurrences(of: "([a-z])([A-Z])", with: "$1 $2", options: .regularExpression, range: nil)
        
        // Capitalize the first letter
        return formatted.prefix(1).uppercased() + formatted.dropFirst()
    }
}