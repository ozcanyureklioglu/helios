import axios from 'axios';

const API_BASE_URL = 'http://localhost:5140/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});

export const api = {
    // Products
    getProducts: async (searchText: string = '') => {
        const response = await apiClient.get(`/products${searchText ? `?searchText=${searchText}` : ''}`);
        return response.data;
    },

    // Warehouse Inventory
    getWarehouseInventories: async (productId?: string) => {
        const url = productId ? `/warehouse-inventories?productId=${productId}` : '/warehouse-inventories';
        const response = await apiClient.get(url);
        return response.data;
    },

    // Reviews
    getReviews: async (productId?: string) => {
        const url = productId ? `/product-reviews?productId=${productId}` : '/product-reviews';
        const response = await apiClient.get(url);
        return response.data;
    },

    // AI Advice
    askAi: async (query: string) => {
        const response = await apiClient.post('/ai/advice', { query });
        return response.data;
    }
};
