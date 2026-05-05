import { useState, useEffect, useRef } from 'react';
import type { Product, CartItem } from './types';
import { Header } from './components/Header';
import { HeroBanner } from './components/HeroBanner';
import { ProductList } from './components/ProductList';
import { CartDrawer } from './components/CartDrawer';
import { useProducts } from './hooks/useProducts';
import { addToCart, fetchCart, updateCartItem, clearCart } from './api';
import './App.css';

export function App() {
  const { products, loading, error } = useProducts();
  const [cartMessage, setCartMessage] = useState<string | null>(null);
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const cartItemCount = cartItems.reduce((sum, item) => sum + item.quantity, 0);

  useEffect(() => {
    refreshCart();
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, []);

  async function refreshCart() {
    try {
      const items = await fetchCart();
      setCartItems(items);
    } catch {
      // cart fetch failure is non-critical; keep existing state
    }
  }

  async function handleAddToCart(product: Product) {
    try {
      await addToCart({ productId: product.id, quantity: 1 });
      await refreshCart();
      setCartMessage(`"${product.name}" added to cart!`);
      if (timerRef.current) clearTimeout(timerRef.current);
      timerRef.current = setTimeout(() => setCartMessage(null), 3000);
    } catch {
      setCartMessage('Failed to add item to cart.');
    }
  }

  async function handleUpdateQuantity(productId: number, quantity: number) {
    try {
      await updateCartItem(productId, quantity);
      await refreshCart();
    } catch {
      setCartMessage('Failed to update cart.');
    }
  }

  async function handleClearCart() {
    try {
      await clearCart();
      await refreshCart();
    } catch {
      setCartMessage('Failed to clear cart.');
    }
  }

  return (
    <div className="app">
      <Header cartItemCount={cartItemCount} onCartOpen={() => setIsCartOpen(true)} />
      <HeroBanner />

      <main className="app__main">
        <h1 className="app__section-heading">Our products</h1>

        {cartMessage && (
          <div className="app__notification" role="status">
            {cartMessage}
          </div>
        )}

        {loading && <p className="app__loading">Loading products…</p>}
        {error && <p className="app__error">Error: {error}</p>}
        {!loading && !error && (
          <ProductList products={products} onAddToCart={handleAddToCart} />
        )}
      </main>

      <CartDrawer
        isOpen={isCartOpen}
        onClose={() => setIsCartOpen(false)}
        items={cartItems}
        onUpdateQuantity={handleUpdateQuantity}
        onClearCart={handleClearCart}
      />
    </div>
  );
}
