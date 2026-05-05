import type { CartItem } from '../../types';

interface CartDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  items: CartItem[];
  onUpdateQuantity: (productId: number, quantity: number) => void;
  onClearCart: () => void;
}

export function CartDrawer({ isOpen, onClose, items, onUpdateQuantity, onClearCart }: CartDrawerProps) {
  if (!isOpen) return null;

  const total = items.reduce((sum, item) => sum + item.totalPrice, 0);

  return (
    <>
      <div
        className="cart-drawer-backdrop"
        onClick={onClose}
        aria-hidden="true"
      />
      <aside className="cart-drawer" aria-label="Shopping cart">
        <div className="cart-drawer__header">
          <h2 className="cart-drawer__title">Your cart</h2>
          <button
            className="cart-drawer__close"
            onClick={onClose}
            aria-label="Close cart"
          >
            ×
          </button>
        </div>

        <div className="cart-drawer__body">
          {items.length === 0 ? (
            <p className="cart-drawer__empty">Your cart is empty</p>
          ) : (
            <ul className="cart-drawer__list">
              {items.map((item) => (
                <li key={item.productId} className="cart-drawer__item">
                  <div className="cart-drawer__item-info">
                    <span className="cart-drawer__item-name">{item.productName}</span>
                    <span className="cart-drawer__item-price">${item.unitPrice.toFixed(2)}</span>
                  </div>
                  <div className="cart-drawer__item-controls">
                    <button
                      className="cart-drawer__qty-btn"
                      onClick={() => onUpdateQuantity(item.productId, item.quantity - 1)}
                      aria-label={`Decrease quantity of ${item.productName}`}
                    >
                      −
                    </button>
                    <span className="cart-drawer__qty">{item.quantity}</span>
                    <button
                      className="cart-drawer__qty-btn"
                      onClick={() => onUpdateQuantity(item.productId, item.quantity + 1)}
                      disabled={item.quantity >= 5}
                      aria-label={`Increase quantity of ${item.productName}`}
                    >
                      +
                    </button>
                    <span className="cart-drawer__item-total">${item.totalPrice.toFixed(2)}</span>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="cart-drawer__footer">
          <div className="cart-drawer__total">
            <span>Total:</span>
            <span>${total.toFixed(2)}</span>
          </div>
          <button
            className="cart-drawer__clear"
            onClick={onClearCart}
            disabled={items.length === 0}
          >
            Clear cart
          </button>
        </div>
      </aside>
    </>
  );
}
