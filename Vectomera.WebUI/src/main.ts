import './style.css';
import { api } from './api';

// --- View Router & State ---
const views = document.querySelectorAll('.view');
const menuItems = document.querySelectorAll('.menu-item');

function switchView(viewId: string) {
    views.forEach(v => v.classList.remove('active'));
    menuItems.forEach(m => m.classList.remove('active'));
    
    document.getElementById(`${viewId}-view`)?.classList.add('active');
    document.querySelector(`[data-view="${viewId}"]`)?.classList.add('active');

    loadViewData(viewId);
}

menuItems.forEach(item => {
    item.addEventListener('click', (e) => {
        e.preventDefault();
        const viewId = (e.currentTarget as HTMLElement).dataset.view;
        if (viewId) switchView(viewId);
    });
});

// --- View Data Loaders ---
const DOM = {
    aiAgent: document.getElementById('ai-agent-view')!,
    products: document.getElementById('products-view')!,
    reviews: document.getElementById('reviews-view')!,
    warehouse: document.getElementById('warehouse-view')!
};

async function loadViewData(viewId: string) {
    switch (viewId) {
        case 'ai-agent':
            renderAiAgent();
            break;
        case 'products':
            await renderProducts();
            break;
        case 'reviews':
            await renderReviews();
            break;
        case 'warehouse':
            await renderWarehouse();
            break;
    }
}

// Helper: Basic Markdown to HTML
function parseMd(text: string) {
    return text
        .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
        .replace(/\n/g, '<br/>')
        .replace(/\[(.*?)\]/g, '<strong>[$1]</strong>');
}

// 1. AI Agent View
let isAiAgentInitialized = false;
function renderAiAgent() {
    if (isAiAgentInitialized) return;
    isAiAgentInitialized = true;

    DOM.aiAgent.innerHTML = `
        <h2 class="page-title">AI Ajan</h2>
        <div class="chat-container">
            <div class="chat-messages" id="chat-messages">
                <div class="message">
                    <div class="message-avatar">🤖</div>
                    <div class="message-content md-content">
                        Merhaba! Vectomera ürün ve stok sistemi hakkında bana istediğini sorabilirsin.
                    </div>
                </div>
            </div>
            <div class="chat-input-container">
                <input type="text" id="chat-input" class="chat-input" placeholder="Bir şeyler sorun..." />
                <button id="chat-send" class="send-btn">➤</button>
            </div>
        </div>
    `;

    const input = document.getElementById('chat-input') as HTMLInputElement;
    const sendBtn = document.getElementById('chat-send') as HTMLButtonElement;
    const messages = document.getElementById('chat-messages')!;

    const sendMessage = async () => {
        const query = input.value.trim();
        if (!query) return;

        // User message
        messages.innerHTML += `
            <div class="message user">
                <div class="message-avatar">👤</div>
                <div class="message-content">${query}</div>
            </div>
        `;
        input.value = '';
        input.disabled = true;
        sendBtn.disabled = true;
        messages.scrollTop = messages.scrollHeight;

        // Loader
        const loaderId = 'loader-' + Date.now();
        messages.innerHTML += `
            <div class="message bot" id="${loaderId}">
                <div class="message-avatar">🤖</div>
                <div class="message-content"><div class="loader" style="margin: 0; width: 16px; height:16px;"></div></div>
            </div>
        `;
        messages.scrollTop = messages.scrollHeight;

        try {
            const res = await api.askAi(query);
            const text = res.data?.answer || res.message || 'Cevap alınamadı.';
            document.getElementById(loaderId)!.innerHTML = `
                <div class="message-avatar">🤖</div>
                <div class="message-content md-content">${parseMd(text)}</div>
            `;
        } catch (err: any) {
            document.getElementById(loaderId)!.innerHTML = `
                <div class="message-avatar">⚠️</div>
                <div class="message-content" style="color: #ef4444;">Hata: ${err.message}</div>
            `;
        } finally {
            input.disabled = false;
            sendBtn.disabled = false;
            input.focus();
            messages.scrollTop = messages.scrollHeight;
        }
    };

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });
}

// 2. Products View
async function renderProducts() {
    DOM.products.innerHTML = `<h2 class="page-title">Ürünler</h2><div class="loader"></div>`;
    try {
        const res = await api.getProducts();
        const products = res.data || [];
        
        const html = products.map((p: any) => `
            <div class="data-card">
                <h3>${p.name}</h3>
                <div class="meta">
                    <span>SKU: ${p.sku}</span>
                </div>
                <p class="desc">${p.description}</p>
            </div>
        `).join('');
        
        DOM.products.innerHTML = `<h2 class="page-title">Ürünler</h2><div class="data-grid">${html}</div>`;
    } catch (err: any) {
        DOM.products.innerHTML = `<h2 class="page-title">Ürünler</h2><p>Hata: ${err.message}</p>`;
    }
}

// 3. Reviews View
async function renderReviews() {
    DOM.reviews.innerHTML = `<h2 class="page-title">Ürün Yorumları</h2><div class="loader"></div>`;
    try {
        const res = await api.getReviews();
        const reviews = res.data || [];
        
        const html = reviews.map((r: any) => `
            <div class="data-card">
                <h3>${r.title}</h3>
                <div class="meta">
                    <span class="badge rating">⭐ ${r.point} / 5</span>
                </div>
                <p class="desc">${r.description}</p>
            </div>
        `).join('');
        
        DOM.reviews.innerHTML = `<h2 class="page-title">Ürün Yorumları</h2><div class="data-grid">${html}</div>`;
    } catch (err: any) {
        DOM.reviews.innerHTML = `<h2 class="page-title">Ürün Yorumları</h2><p>Hata: ${err.message}</p>`;
    }
}

// 4. Warehouse View
async function renderWarehouse() {
    DOM.warehouse.innerHTML = `<h2 class="page-title">Depo ve Ürünler</h2><div class="loader"></div>`;
    try {
        const res = await api.getWarehouseInventories();
        const inv = res.data || [];
        
        const html = inv.map((i: any) => `
            <div class="data-card">
                <h3>${i.warehouseName}</h3>
                <div class="meta">
                    <span class="badge stock">Stok: ${i.availableStock}</span>
                    <span class="badge price">₺${i.price}</span>
                </div>
                <p class="desc"><strong>${i.productName}</strong><br/>${i.description}</p>
            </div>
        `).join('');
        
        DOM.warehouse.innerHTML = `<h2 class="page-title">Depo ve Ürünler</h2><div class="data-grid">${html}</div>`;
    } catch (err: any) {
        DOM.warehouse.innerHTML = `<h2 class="page-title">Depo ve Ürünler</h2><p>Hata: ${err.message}</p>`;
    }
}

// Init
switchView('ai-agent');
