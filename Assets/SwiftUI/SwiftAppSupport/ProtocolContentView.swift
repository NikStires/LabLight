import SwiftUI
import AVKit

struct ProtocolContentView: View {
    let contentItems: [ContentItem]
    
    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 20) {
                ForEach(contentItems) { item in
                    contentView(for: item)
                }
            }
            .padding()
        }
    }
    
    @ViewBuilder
    private func contentView(for item: ContentItem) -> some View {
        switch item.contentType.lowercased() {
        case "text":
            if let text = item.properties["text"], !text.isEmpty {
                TextContentView(text: text)
            }
        
        case "image":
            if let url = item.properties["url"], !url.isEmpty {
                ImageContentView(imageName: url)
            }
        
        case "video":
            if let url = item.properties["url"], !url.isEmpty {
                VideoContentView(url)
            }
        
        case "timer":
            if let durationStr = item.properties["durationInSeconds"],
               let duration = Int(durationStr),
               duration > 0 {
                TimerContentView(duration)
            }
        
        case "webpage":
            if let url = item.properties["url"], !url.isEmpty {
                SafariContentView(defaultUrlString: url)
            }
            
        default:
            Text("Unsupported content type: \(item.contentType)")
                .foregroundColor(.red)
                .padding()
                .background(Color(.systemGray6))
                .cornerRadius(8)
        }
    }
}

struct TextContentView: View {
    let text: String
    
    var body: some View {
        Text(text)
            .font(.body)
            .padding()
            .background(Color(.systemGray6))
            .cornerRadius(8)
    }
}

struct ImageContentView: View {
    let imageName: String
    
    var body: some View {
        if let url = URL(string: imageName), UIApplication.shared.canOpenURL(url) {
            // Remote Image
            AsyncImage(url: url) { phase in
                switch phase {
                case .empty:
                    ProgressView()
                case .success(let image):
                    image
                        .resizable()
                        .scaledToFit()
                case .failure:
                    Image(systemName: "photo")
                        .resizable()
                        .scaledToFit()
                        .foregroundColor(.gray)
                @unknown default:
                    EmptyView()
                }
            }
            .frame(maxWidth: .infinity)
        } else {
            // Local Image
            Image(imageName)
                .resizable()
                .scaledToFit()
                .frame(maxWidth: .infinity)
        }
    }
}
