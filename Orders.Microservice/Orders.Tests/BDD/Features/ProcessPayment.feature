Feature: Process Payment Webhook
    As a payment gateway
    I want to notify the system about payment status
    So that orders can be processed accordingly

Scenario: Successfully process paid payment
    Given an order with id 1 exists with status "RECEIVED"
    When I receive a webhook with status "PAID" and paymentId "pay_123"
    Then the payment should be processed successfully
    And the order status should be "IN_PREPARATION"
    And the order payment status should be "PAID"
    And the order should have paymentId "pay_123"

Scenario: Process refused payment
    Given an order with id 2 exists with status "RECEIVED"
    When I receive a webhook with status "REFUSED" and paymentId "pay_456"
    Then the payment should be processed successfully
    And the order status should remain "RECEIVED"
    And the order payment status should be "REFUSED"

Scenario: Handle invalid order id
    When I receive a webhook with invalid orderId "invalid"
    Then the webhook processing should fail
    And the error message should contain "OrderId invalido"

Scenario: Handle non-existent order
    When I receive a webhook for non-existent order "00000000-0000-0000-0000-000000000999"
    Then the webhook processing should fail
    And the error message should contain "nao encontrado"

Scenario: Handle duplicate webhook
    Given an order with id 3 exists with paymentId "pay_789" and status "PAID"
    When I receive a duplicate webhook with paymentId "pay_789"
    Then the payment should be processed successfully
    And the message should contain "ja processado"
    And the order should not be updated again