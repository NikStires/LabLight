import SwiftUI
import WebKit

struct SafariContentView: View {
    let defaultUrlString: String?
    @State private var urlString: String = ""
    @State private var currentUrl: URL?
    @FocusState private var isEditing: Bool

    init(defaultUrlString: String? = nil) {
        self.defaultUrlString = defaultUrlString
    }

    var body: some View {
        VStack {
            HStack {
                TextField("Enter URL or search term", text: $urlString, onEditingChanged: { editing in
                    if editing {
                        isEditing = true
                    }
                }, onCommit: loadUrl)
                .textFieldStyle(RoundedBorderTextFieldStyle())
                .padding()
                .focused($isEditing)

                Button(action: loadUrl) {
                    Image(systemName: "magnifyingglass")
                        .padding(.horizontal)
                        .padding(.vertical, 8)
                        .foregroundColor(.white)
                        .cornerRadius(8)
                }
                .padding(.trailing)
            }

            if let url = currentUrl {
                WebView(url: url)
                    .edgesIgnoringSafeArea(.all)
            } else {
                Text("Enter a valid URL or search term to begin browsing.")
                    .padding()
            }
        }
        .onAppear {
            if let defaultUrlString = defaultUrlString, let url = URL(string: defaultUrlString) {
                currentUrl = url
                urlString = defaultUrlString
            }
        }
    }

    private func loadUrl() {
        if let url = URL(string: urlString), UIApplication.shared.canOpenURL(url) {
            currentUrl = url
        } else {
            // If the input is not a valid URL, treat it as a search query
            let query = urlString.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
            if let searchUrl = URL(string: "https://www.google.com/search?q=\(query)") {
                currentUrl = searchUrl
            }
        }
    }
}

struct WebView: UIViewControllerRepresentable {
    let url: URL

    func makeUIViewController(context: Context) -> UIViewController {
        let webView = WKWebView()
        let viewController = UIViewController()
        viewController.view = webView
        let request = URLRequest(url: url)
        webView.load(request)
        return viewController
    }

    func updateUIViewController(_ uiViewController: UIViewController, context: Context) {
        if let webView = uiViewController.view as? WKWebView {
            webView.load(URLRequest(url: url))
        }
    }
}

struct SafariContentView_Previews: PreviewProvider {
    static var previews: some View {
        SafariContentView(defaultUrlString: "https://www.apple.com")
    }
}
