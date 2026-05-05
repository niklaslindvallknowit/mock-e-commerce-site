import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CartDrawer } from '../../../../src/frontend/src/components/CartDrawer/CartDrawer';
import type { CartItem } from '../../../../src/frontend/src/types';

const makeItem = (overrides: Partial<CartItem> = {}): CartItem => ({
  productId: 1,
  productName: 'Wireless Headphones',
  unitPrice: 79.99,
  quantity: 1,
  totalPrice: 79.99,
  ...overrides,
});

const defaultProps = {
  isOpen: true,
  onClose: vi.fn(),
  items: [],
  onUpdateQuantity: vi.fn(),
  onClearCart: vi.fn(),
};

describe('CartDrawer', () => {
  beforeEach(() => vi.clearAllMocks());

  it('renders empty state when items is empty', () => {
    render(<CartDrawer {...defaultProps} />);
    expect(screen.getByText('Your cart is empty')).toBeInTheDocument();
  });

  it('renders item name, unit price, quantity and total when items present', () => {
    const item = makeItem({ quantity: 2, totalPrice: 159.98 });
    render(<CartDrawer {...defaultProps} items={[item]} />);

    expect(screen.getByText('Wireless Headphones')).toBeInTheDocument();
    expect(screen.getByText('$79.99')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    // $159.98 appears in both item total and footer total — both are correct
    expect(screen.getAllByText('$159.98')).toHaveLength(2);
  });

  it('calls onUpdateQuantity with decremented value when − is clicked', async () => {
    const onUpdateQuantity = vi.fn();
    const item = makeItem({ quantity: 3 });
    render(<CartDrawer {...defaultProps} items={[item]} onUpdateQuantity={onUpdateQuantity} />);

    await userEvent.click(screen.getByLabelText(`Decrease quantity of ${item.productName}`));

    expect(onUpdateQuantity).toHaveBeenCalledWith(1, 2);
  });

  it('calls onUpdateQuantity with 0 when − clicked at quantity 1 (removes item)', async () => {
    const onUpdateQuantity = vi.fn();
    const item = makeItem({ quantity: 1 });
    render(<CartDrawer {...defaultProps} items={[item]} onUpdateQuantity={onUpdateQuantity} />);

    await userEvent.click(screen.getByLabelText(`Decrease quantity of ${item.productName}`));

    expect(onUpdateQuantity).toHaveBeenCalledWith(1, 0);
  });

  it('+ button is disabled when quantity is 5', () => {
    const item = makeItem({ quantity: 5 });
    render(<CartDrawer {...defaultProps} items={[item]} />);

    expect(screen.getByLabelText(`Increase quantity of ${item.productName}`)).toBeDisabled();
  });

  it('calls onClearCart when "Clear cart" is clicked', async () => {
    const onClearCart = vi.fn();
    const item = makeItem();
    render(<CartDrawer {...defaultProps} items={[item]} onClearCart={onClearCart} />);

    await userEvent.click(screen.getByRole('button', { name: 'Clear cart' }));

    expect(onClearCart).toHaveBeenCalledOnce();
  });

  it('"Clear cart" is disabled when cart is empty', () => {
    render(<CartDrawer {...defaultProps} items={[]} />);
    expect(screen.getByRole('button', { name: 'Clear cart' })).toBeDisabled();
  });

  it('calls onClose when backdrop is clicked', async () => {
    const onClose = vi.fn();
    render(<CartDrawer {...defaultProps} onClose={onClose} items={[makeItem()]} />);

    await userEvent.click(document.querySelector('.cart-drawer-backdrop')!);

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose when × button is clicked', async () => {
    const onClose = vi.fn();
    render(<CartDrawer {...defaultProps} onClose={onClose} />);

    await userEvent.click(screen.getByLabelText('Close cart'));

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('displays correct cart total', () => {
    const items = [
      makeItem({ productId: 1, productName: 'A', unitPrice: 10, quantity: 2, totalPrice: 20 }),
      makeItem({ productId: 2, productName: 'B', unitPrice: 5, quantity: 3, totalPrice: 15 }),
    ];
    render(<CartDrawer {...defaultProps} items={items} />);

    expect(screen.getByText('$35.00')).toBeInTheDocument();
  });

  it('returns null when isOpen is false', () => {
    const { container } = render(<CartDrawer {...defaultProps} isOpen={false} />);
    expect(container.firstChild).toBeNull();
  });
});
